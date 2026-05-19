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
        // 손목(W)을 기준 0촌으로, 팔꿈치(E) 방향이 양수.
        // ⚠️ 키는 AcupointDB의 순번 ID (전통 경혈 번호가 아님).
        // 표준 동의보감 경혈 위치 기준.
        private static readonly Dictionary<int, float> AcupointCunOffset = new Dictionary<int, float>
        {
            // ── 수태음폐경 (LU) — ID 순번 기준 ──
            { 11, -2.0f },  // 소상(少商) LU11 — 엄지 손끝 (손목 바깥 2촌 추정)
            { 12, -0.8f },  // 어제(魚際) LU10 — 엄지 장골 중점 (손바닥)
            { 13,  0.0f },  // 태연(太淵) LU9  — 손목 횡문 요골 측
            { 14,  1.5f },  // 경거(經渠) LU8  — 손목에서 1촌
            { 7,   1.5f },  // 열결(列缺) LU7  — 손목에서 1.5촌
            { 6,   7.0f },  // 공최(孔最) LU6  — 손목에서 7촌
            { 5,  12.0f },  // 척택(尺澤) LU5  — 팔꿈치 주름 (12촌)
            // ── 수궐음심포경 (PC) ──
            { 101, 0.0f },  // 대릉(大陵) PC7  — 손목
            { 102, 2.0f },  // 내관(內關) PC6  — 손목에서 2촌
            { 103, 5.0f },  // 간사(間使) PC5  — 손목에서 5촌
            // ── 수소음심경 (HT) ──
            { 201, 0.0f },  // 신문(神門) HT7  — 손목 내측
            { 202, 0.5f },  // 음극(陰隙) HT6  — 손목에서 0.5촌
            // ── 수소양삼초경 (TE) ──
            { 301, 2.0f },  // 외관(外關) TE5  — 손목 배측에서 2촌
            { 302, 0.0f },  // 양지(陽池) TE4  — 손목 횡문
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
