using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace MSG.AR
{
    /// <summary>
    /// [WBS 3.3 / 3.4] DynamicCunCalibrator (동적 촌도 캘리브레이션)
    /// 손목(Wrist)과 팔꿈치(Elbow) 두 점을 입력받아 1촌(Cun) 절대 척도를 계산.
    /// 이후 혈자리 ID별 촌도 오프셋으로 정확한 3D 세계 좌표를 계산하여 반환.
    /// </summary>
    public class DynamicCunCalibrator : MonoBehaviour
    {
        public static DynamicCunCalibrator Instance { get; private set; }

        [Header("AR 세팅")]
        [SerializeField] private AROcclusionManager occlusionManager;

        [Header("캘리브레이션 파라미터")]
        [Tooltip("팔꿈치~손목 = 12촌 규칙 기반")]
        [SerializeField] private float bodyRatioRuleInCun = 12f;

        // ── 결과 ──────────────────────────────────────────────────────
        /// <summary>1촌(1 Cun)의 절대 길이(미터)</summary>
        public float OneCunInMeters { get; private set; } = 0.022f; // 기본 2.2cm
        public bool IsCalibrated { get; private set; } = false;

        /// <summary>손목 3D 좌표 (캘리브레이션 후 유효)</summary>
        public Vector3 WristPosition { get; private set; }
        /// <summary>팔꿈치 3D 좌표 (캘리브레이션 후 유효)</summary>
        public Vector3 ElbowPosition { get; private set; }
        /// <summary>손목 → 팔꿈치 방향 벡터 (정규화)</summary>
        public Vector3 ArmDirection { get; private set; }

        // ── 1D 칼만 필터 (안정화용) ────────────────────────────────────
        private KalmanFilter1D _kalman = new KalmanFilter1D(0.01f, 0.1f);

        // ── 측정 버퍼 (30프레임) ──────────────────────────────────────
        private readonly Queue<float> _measurementBuffer = new Queue<float>();
        private const int BUFFER_SIZE = 30;

        // ── 혈자리 촌도 오프셋 테이블 ────────────────────────────────
        // 기준점: 손목 횡문(손목 주름) = 0촌, 팔꿈치 방향 = 양수(+), 손가락 방향 = 음수(-)
        // 팔꿈치~손목 = 12촌 규칙 기반. 표준 동의보감 경혈 위치 기준.
        // ⚠️ 키는 Acupoints.csv의 ID 컬럼 (1~361 순번).
        //
        // [팔/손 경락 전체 커버]
        // 수태음폐경(LU) ID 1~11, 수양명대장경(LI) ID 12~31
        // 수소음심경(HT) ID 98~106, 수태양소장경(SI) ID 107~125
        // 수궐음심포경(PC) ID 220~228, 수소양삼초경(TE) ID 229~251
        //
        // [진단 처방에 등장하는 비팔 경락 혈자리]
        // 족양명위경(ST), 족태음비경(SP), 족태양방광경(BL), 족소양담경(GB),
        // 임맥(CV), 독맥(GV) 등 — 팔 기준 오프셋 적용 불가이므로 0촌(손목) 폴백 처리.
        // AR 씬에서는 손목 위치에 마커가 표시되며, 향후 전신 랜드마크 확장 시 개선 예정.
        private static readonly Dictionary<int, float> AcupointCunOffset = new Dictionary<int, float>
        {
            // ════════════════════════════════════════════════════════
            //  수태음폐경 (Lung Meridian, LU) — ID 1~11
            //  기준: 손목 횡문(태연 LU9) = 0촌
            // ════════════════════════════════════════════════════════
            { 1,  16.0f },  // 중부(中府) LU1  — 쇄골 아래 (팔꿈치 위 4촌 추정)
            { 2,  15.5f },  // 운문(雲門) LU2  — 쇄골 바깥쪽
            { 3,  14.0f },  // 천부(天府) LU3  — 겨드랑이 아래 3촌
            { 4,  13.0f },  // 협백(俠白) LU4  — 천부에서 1촌 아래
            { 5,  12.0f },  // 척택(尺澤) LU5  — 팔꿈치 주름 (12촌)
            { 6,   7.0f },  // 공최(孔最) LU6  — 손목에서 7촌
            { 7,   1.5f },  // 열결(列缺) LU7  — 손목에서 1.5촌
            { 8,   1.0f },  // 경거(經渠) LU8  — 손목에서 1촌
            { 9,   0.0f },  // 태연(太淵) LU9  — 손목 횡문 (기준점)
            { 10, -0.8f },  // 어제(魚際) LU10 — 엄지 장골 중점 (손바닥)
            { 11, -2.0f },  // 소상(少商) LU11 — 엄지 손끝

            // ════════════════════════════════════════════════════════
            //  수양명대장경 (Large Intestine Meridian, LI) — ID 12~31
            //  기준: 손목 배측 횡문 = 0촌
            // ════════════════════════════════════════════════════════
            { 12, -1.5f },  // 상양(商陽) LI1  — 검지 손끝
            { 13, -1.0f },  // 이간(二間) LI2  — 검지 본절 앞
            { 14, -0.5f },  // 삼간(三間) LI3  — 검지 본절 뒤
            { 15,  0.0f },  // 합곡(合谷) LI4  — 엄지·검지 사이 (손목 기준 0촌 근사)
            { 16,  0.5f },  // 양계(陽溪) LI5  — 손목 배측 횡문
            { 17,  3.0f },  // 편력(偏歷) LI6  — 손목에서 3촌
            { 18,  5.0f },  // 온류(溫溜) LI7  — 손목에서 5촌
            { 19,  8.0f },  // 하렴(下廉) LI8  — 팔꿈치에서 4촌 아래
            { 20,  9.0f },  // 상렴(上廉) LI9  — 팔꿈치에서 3촌 아래
            { 21, 10.0f },  // 수삼리(手三里) LI10 — 팔꿈치에서 2촌 아래
            { 22, 12.0f },  // 곡지(曲池) LI11 — 팔꿈치 주름 바깥쪽 끝 (12촌)
            { 23, 13.0f },  // 주료(肘髎) LI12 — 곡지 위 1촌
            { 24, 15.0f },  // 수오리(手五里) LI13 — 곡지에서 3촌 위
            { 25, 19.0f },  // 비노(臂臑) LI14 — 곡지에서 7촌 위
            { 26, 22.0f },  // 견우(肩髃) LI15 — 어깨 (팔꿈치 위 10촌 추정)
            { 27, 23.0f },  // 거골(巨骨) LI16 — 쇄골 바깥쪽
            { 28, 24.0f },  // 천정(天鼎) LI17 — 목 옆
            { 29, 24.5f },  // 부돌(扶突) LI18 — 목 옆
            { 30,  0.0f },  // 화료(禾髎) LI19 — 코 옆 (팔 기준 불가, 손목 폴백)
            { 31,  0.0f },  // 영향(迎香) LI20 — 콧날개 (팔 기준 불가, 손목 폴백)

            // ════════════════════════════════════════════════════════
            //  수소음심경 (Heart Meridian, HT) — ID 98~106
            //  기준: 손목 내측 횡문(신문 HT7) = 0촌
            // ════════════════════════════════════════════════════════
            { 98,  16.0f }, // 극천(極泉) HT1  — 겨드랑이
            { 99,  12.0f }, // 청령(靑靈) HT2  — 팔꿈치 위 3촌
            { 100, 12.0f }, // 소해(少海) HT3  — 팔꿈치 주름 내측 끝 (12촌)
            { 101,  1.5f }, // 영도(靈道) HT4  — 손목에서 1.5촌
            { 102,  1.0f }, // 통리(通里) HT5  — 손목에서 1촌
            { 103,  0.5f }, // 음극(陰隙) HT6  — 손목에서 0.5촌
            { 104,  0.0f }, // 신문(神門) HT7  — 손목 내측 횡문 (기준점)
            { 105, -0.5f }, // 소부(少府) HT8  — 손바닥 (손목 안쪽)
            { 106, -1.5f }, // 소충(少衝) HT9  — 새끼손가락 끝

            // ════════════════════════════════════════════════════════
            //  수태양소장경 (Small Intestine Meridian, SI) — ID 107~125
            //  기준: 손목 배측 척측(양곡 SI5) = 0촌
            // ════════════════════════════════════════════════════════
            { 107, -1.5f }, // 소택(少澤) SI1  — 새끼손가락 끝
            { 108, -1.0f }, // 전곡(前谷) SI2  — 새끼손가락 본절 앞
            { 109, -0.5f }, // 후계(後谿) SI3  — 새끼손가락 본절 뒤
            { 110,  0.0f }, // 완골(腕骨) SI4  — 손목 척측
            { 111,  0.5f }, // 양곡(陽谷) SI5  — 손목 배측 척측 횡문
            { 112,  1.0f }, // 양로(養老) SI6  — 손목에서 1촌
            { 113,  5.0f }, // 지정(支正) SI7  — 손목에서 5촌
            { 114, 12.0f }, // 소해(小海) SI8  — 팔꿈치 내측 (12촌)
            { 115, 13.0f }, // 견정(肩貞) SI9  — 겨드랑이 뒤 1촌
            { 116, 14.0f }, // 노유(臑兪) SI10 — 견갑골 가시 아래
            { 117, 15.0f }, // 천종(天宗) SI11 — 견갑골 중앙
            { 118, 15.5f }, // 병풍(秉風) SI12 — 견갑골 가시 위
            { 119, 16.0f }, // 곡원(曲垣) SI13 — 견갑골 가시 내측
            { 120, 16.5f }, // 견외유(肩外兪) SI14 — 등 1번 척추 옆
            { 121, 17.0f }, // 견중유(肩中兪) SI15 — 7번 경추 옆
            { 122, 17.5f }, // 천창(天窓) SI16 — 목 옆
            { 123, 18.0f }, // 천용(天容) SI17 — 아래턱 뒤
            { 124,  0.0f }, // 권료(颧髎) SI18 — 광대뼈 (팔 기준 불가)
            { 125,  0.0f }, // 청궁(聽宮) SI19 — 귀 앞 (팔 기준 불가)

            // ════════════════════════════════════════════════════════
            //  수궐음심포경 (Pericardium Meridian, PC) — ID 220~228
            //  기준: 손목 전면 횡문(대릉 PC7) = 0촌
            // ════════════════════════════════════════════════════════
            { 220, 16.0f }, // 천지(天池) PC1  — 가슴 (팔 기준 불가, 폴백)
            { 221, 14.0f }, // 천천(天泉) PC2  — 겨드랑이 앞 2촌
            { 222, 12.0f }, // 곡택(曲澤) PC3  — 팔꿈치 주름 중앙 (12촌)
            { 223,  5.0f }, // 극문(郄門) PC4  — 손목에서 5촌
            { 224,  3.0f }, // 간사(間使) PC5  — 손목에서 3촌
            { 225,  2.0f }, // 내관(內關) PC6  — 손목에서 2촌 ★ 핵심 혈자리
            { 226,  0.0f }, // 대릉(大陵) PC7  — 손목 전면 횡문 (기준점)
            { 227, -0.5f }, // 노궁(勞宮) PC8  — 손바닥 중앙
            { 228, -1.5f }, // 중충(中衝) PC9  — 가운데 손가락 끝

            // ════════════════════════════════════════════════════════
            //  수소양삼초경 (Triple Energizer Meridian, TE) — ID 229~251
            //  기준: 손목 배측 횡문(양지 TE4) = 0촌
            // ════════════════════════════════════════════════════════
            { 229, -1.5f }, // 관충(關衝) TE1  — 넷째 손가락 끝
            { 230, -1.0f }, // 액문(液門) TE2  — 넷째·다섯째 손가락 사이
            { 231, -0.5f }, // 중저(中渚) TE3  — 손등 넷째·다섯째 사이
            { 232,  0.0f }, // 양지(陽池) TE4  — 손목 배측 횡문 (기준점)
            { 233,  2.0f }, // 외관(外關) TE5  — 손목에서 2촌 ★ 핵심 혈자리
            { 234,  3.0f }, // 지구(支溝) TE6  — 손목에서 3촌
            { 235,  3.0f }, // 회종(會宗) TE7  — 지구 바깥쪽
            { 236,  4.0f }, // 삼양락(三陽絡) TE8 — 손목에서 4촌
            { 237,  7.0f }, // 사독(四瀆) TE9  — 팔꿈치에서 5촌 아래
            { 238, 13.0f }, // 천정(天井) TE10 — 팔꿈치 위 1촌
            { 239, 14.0f }, // 청랭연(淸冷淵) TE11 — 팔꿈치 위 2촌
            { 240, 17.0f }, // 소락(消濼) TE12 — 팔꿈치 위 5촌
            { 241, 19.0f }, // 노회(臑會) TE13 — 팔꿈치 위 7촌
            { 242, 22.0f }, // 견료(肩髎) TE14 — 어깨 뒤쪽
            { 243, 23.0f }, // 천료(天髎) TE15 — 견갑골 위
            { 244, 24.0f }, // 천유(天牖) TE16 — 목 옆
            { 245,  0.0f }, // 예풍(翳風) TE17 — 귓불 뒤 (팔 기준 불가)
            { 246,  0.0f }, // 계맥(瘈脈) TE18 — 귀 뒤 (팔 기준 불가)
            { 247,  0.0f }, // 노식(顱息) TE19 — 귀 뒤 (팔 기준 불가)
            { 248,  0.0f }, // 각손(角孫) TE20 — 귀 위 (팔 기준 불가)
            { 249,  0.0f }, // 이문(耳門) TE21 — 귀 앞 (팔 기준 불가)
            { 250,  0.0f }, // 화료(禾髎) TE22 — 귀 앞 (팔 기준 불가)
            { 251,  0.0f }, // 사죽공(絲竹空) TE23 — 눈썹 끝 (팔 기준 불가)

            // ════════════════════════════════════════════════════════
            //  진단 처방에 등장하는 비팔 경락 혈자리
            //  (팔/손 이외 신체 부위 — 손목 위치 폴백, 향후 전신 랜드마크 확장 예정)
            // ════════════════════════════════════════════════════════

            // 족양명위경 (Stomach Meridian, ST)
            { 33,  0.0f },  // 사백(四白) ST2  — 눈 아래
            { 56,  0.0f },  // 천추(天樞) ST25 — 배꼽 옆
            { 67,  0.0f },  // 족삼리(足三里) ST36 — 무릎 아래 3촌 ★ 핵심
            { 65,  0.0f },  // 양구(梁丘) ST34 — 무릎 위 2촌
            { 75,  0.0f },  // 내정(內庭) ST44 — 발가락 사이

            // 족태음비경 (Spleen Meridian, SP)
            { 82,  0.0f },  // 삼음교(三陰交) SP6 — 발목 위 3촌 ★ 핵심
            { 85,  0.0f },  // 음릉천(陰陵泉) SP9 — 무릎 아래 내측
            { 86,  0.0f },  // 혈해(血海) SP10 — 무릎 위 내측
            { 91,  0.0f },  // 대횡(大橫) SP15 — 배꼽 옆

            // 족태양방광경 (Bladder Meridian, BL)
            // { 109 } — 후계(後谿) SI3으로 이미 등록됨 (중복 방지)
            { 135,  0.0f }, // 천주(天柱) BL10 — 뒷목
            { 136,  0.0f }, // 대저(大杼) BL11 — 등 1번 척추 옆
            { 138,  0.0f }, // 폐유(肺兪) BL13 — 등 3번 척추 옆
            { 148,  0.0f }, // 신유(腎兪) BL23 — 허리 2번 척추 옆 ★ 핵심
            { 153,  0.0f }, // 방광유(膀胱兪) BL28 — 엉치뼈 옆
            { 165,  0.0f }, // 위중(委中) BL40 — 오금 중앙 ★ 핵심
            { 168,  0.0f }, // 고황(膏肓) BL43 — 등 4번 척추 옆 3촌
            { 181,  0.0f }, // 승근(承筋) BL56 — 종아리 중앙
            { 182,  0.0f }, // 승산(承山) BL57 — 종아리 하부 ★ 핵심
            { 183,  0.0f }, // 비양(飛揚) BL58 — 종아리 외측

            // 족소음신경 (Kidney Meridian, KI)
            { 193,  0.0f }, // 용천(湧泉) KI1  — 발바닥
            { 195,  0.0f }, // 태계(太谿) KI3  — 발목 내측

            // 족소양담경 (Gallbladder Meridian, GB)
            { 259,  0.0f }, // 솔곡(率谷) GB8  — 귀 위 머리
            { 271,  0.0f }, // 풍지(風池) GB20 — 뒷목 아래 ★ 핵심
            { 272,  0.0f }, // 견정(肩井) GB21 — 어깨 중앙 ★ 핵심
            { 281,  0.0f }, // 환도(環跳) GB30 — 엉덩이 외측
            { 285,  0.0f }, // 양릉천(陽陵泉) GB34 — 무릎 아래 외측 ★ 핵심
            { 291,  0.0f }, // 구허(丘墟) GB40 — 발목 외측

            // 족궐음간경 (Liver Meridian, LR)
            { 298,  0.0f }, // 태충(太衝) LR3  — 발등

            // 임맥 (Conception Vessel, CV)
            { 321,  0.0f }, // 중완(中脘) CV12 — 배꼽 위 4촌 ★ 핵심
            { 326,  0.0f }, // 단중(膻中) CV17 — 가슴 중앙

            // 독맥 (Governing Vessel, GV)
            { 337,  0.0f }, // 명문(命門) GV4  — 허리 2번 척추 아래
            { 342,  0.0f }, // 지양(至陽) GV9  — 등 7번 척추 아래
            { 347,  0.0f }, // 대추(大椎) GV14 — 7번 경추 아래 ★ 핵심
            { 353,  0.0f }, // 백회(百會) GV20 — 머리 꼭대기 ★ 핵심
            { 358,  0.0f }, // 소료(素髎) GV25 — 코끝

            // 기타 두면부 혈자리
            { 128,  0.0f }, // 미충(眉衝) BL7  — 눈썹 위 머리카락 선 (팔 기준 불가)
            { 185,  0.0f }, // 곤륜(崑崙) BL60 — 발목 외측 (팔 기준 불가)
            { 252,  0.0f }, // 동자료(瞳子髎) GB1 — 눈 바깥쪽 (팔 기준 불가)
            { 265,  0.0f }, // 양백(陽白) GB14 — 눈썹 위 (팔 기준 불가)
            { 332,  0.0f }, // 염천(廉泉) CV23 — 목 앞 (팔 기준 불가)
        };


        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// [WBS 3.3] 손목과 팔꿈치 3D 좌표로 1촌 길이를 계산.
        /// 팔꿈치~손목 = 12촌 규칙 기반.
        /// </summary>
        public void Calibrate(Vector3 wristPos, Vector3 elbowPos, float distanceInCun = -1f)
        {
            if (distanceInCun < 0) distanceInCun = bodyRatioRuleInCun;

            WristPosition = wristPos;
            ElbowPosition = elbowPos;
            ArmDirection = (elbowPos - wristPos).normalized;

            // 유클리드 거리
            float distanceMeters = Vector3.Distance(wristPos, elbowPos);

            // 1촌 절대 척도
            float rawCun = distanceMeters / distanceInCun;

            // 버퍼에 추가 (30프레임 평균)
            _measurementBuffer.Enqueue(rawCun);
            if (_measurementBuffer.Count > BUFFER_SIZE)
                _measurementBuffer.Dequeue();

            // 버퍼 평균
            float sum = 0f;
            foreach (float v in _measurementBuffer) sum += v;
            float averaged = sum / _measurementBuffer.Count;

            // 1D 칼만 필터 적용 (떨림 제거)
            OneCunInMeters = _kalman.Update(averaged);
            IsCalibrated = true;

            Debug.Log($"[DynamicCunCalibrator] ✅ 캘리브레이션 완료: 1촌 = {OneCunInMeters * 100f:F2}cm | 팔길이 = {distanceMeters * 100f:F1}cm");
        }

        /// <summary>
        /// [WBS 3.4] 혈자리 ID와 캘리브레이션된 손목/팔꿈치 정보를 기반으로
        /// 해당 혈자리의 실제 3D 세계 좌표를 반환.
        /// </summary>
        /// <param name="acupointId">혈자리 DB ID</param>
        /// <returns>혈자리의 3D 월드 좌표. 미캘리브레이션 시 WristPosition 반환.</returns>
        public Vector3 GetAcupointWorldPosition(int acupointId)
        {
            if (!IsCalibrated)
            {
                Debug.LogWarning("[DynamicCunCalibrator] 아직 캘리브레이션이 완료되지 않았습니다.");
                return WristPosition;
            }

            float cunOffset = GetCunOffset(acupointId);
            return CunToWorldPosition(WristPosition, ArmDirection, cunOffset);
        }

        /// <summary>
        /// 혈자리 ID로 촌도 오프셋을 반환. DB에 없으면 0.0(손목) 반환.
        /// </summary>
        public float GetCunOffset(int acupointId)
        {
            if (AcupointCunOffset.TryGetValue(acupointId, out float offset))
                return offset;

            // 알 수 없는 혈자리는 기본적으로 손목 기준 3촌 위치로 폴백
            Debug.LogWarning($"[DynamicCunCalibrator] 혈자리 ID {acupointId}의 촌도 오프셋이 정의되어 있지 않습니다. 3촌으로 폴백.");
            return 3.0f;
        }

        /// <summary>
        /// [WBS 3.4] 피부 위 촌도 거리를 3D 공간 벡터로 변환.
        /// 비선형 곡률 보정: 아크 길이 적분식 적용 (간략화 버전).
        /// </summary>
        public Vector3 CunToWorldPosition(Vector3 origin, Vector3 direction,
            float cunDistance, float curvatureFactor = 1.05f)
        {
            // 실제 호의 길이(Arc Length) 보정: 직선 * 곡률 계수
            float arcLength = cunDistance * OneCunInMeters * curvatureFactor;
            return origin + direction.normalized * arcLength;
        }

        /// <summary>
        /// 캘리브레이션을 수동으로 강제 주입 (에디터 폴백용).
        /// wristPos와 elbowPos로부터 방향과 촌도를 계산.
        /// </summary>
        public void ForceCalibrate(Vector3 wristPos, Vector3 elbowPos)
        {
            Calibrate(wristPos, elbowPos, 12f);
        }

        /// <summary>
        /// 상태 초기화 (재캘리브레이션 준비)
        /// </summary>
        public void Reset()
        {
            IsCalibrated = false;
            _measurementBuffer.Clear();
            Debug.Log("[DynamicCunCalibrator] 캘리브레이션 상태 초기화.");
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  [WBS 3.4] 1D 칼만 필터 구현
    //  IMU 자이로 틸트 보상 포함 (측정 노이즈 공분산 Q/R 조정으로 제어)
    // ─────────────────────────────────────────────────────────────────
    public class KalmanFilter1D
    {
        private float _q; // 프로세스 노이즈 공분산 (Q)
        private float _r; // 측정 노이즈 공분산 (R)
        private float _x; // 추정값
        private float _p; // 오차 공분산

        public KalmanFilter1D(float q, float r, float initialValue = 0f)
        {
            _q = q;
            _r = r;
            _x = initialValue;
            _p = 1f;
        }

        public float Update(float measurement)
        {
            // 예측 단계
            _p += _q;

            // 칼만 게인
            float k = _p / (_p + _r);

            // 업데이트 단계
            _x += k * (measurement - _x);
            _p *= (1f - k);

            return _x;
        }

        /// <summary>자이로 틸트 보상: 기울기 각도에 따라 측정 노이즈를 동적 조정.</summary>
        public void AdjustForTilt(float tiltAngleDegrees)
        {
            // 기울기가 클수록 측정 신뢰도 낮음 → R 증가
            _r = Mathf.Lerp(0.05f, 0.5f, tiltAngleDegrees / 90f);
        }
    }
}
