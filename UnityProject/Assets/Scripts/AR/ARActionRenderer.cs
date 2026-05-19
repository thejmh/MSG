using System.Collections.Generic;
using UnityEngine;
using MSG.Services;

namespace MSG.AR
{
    // ════════════════════════════════════════════════════════════════
    //  [WBS 3.5] ARActionRenderer (Strategy Pattern)
    //  [Self-Healing & Auto-Fallback Engine]
    //  1. 마커 프리팹이나 이펙트 프리팹이 누락되어도 즉시 에메랄드 3D Sphere 및
    //     시뮬레이터용 꼬마 반응 구체를 자동 주조하여 크래시 없는 논스톱 씬을 유지합니다.
    //  2. LineRenderer가 공백일 시 런타임에 직접 생성해 빛나는 경로선을 매끄럽게 연결합니다.
    // ════════════════════════════════════════════════════════════════
    public class ARActionRenderer : MonoBehaviour
    {
        [Header("마커 프리팹 (인스펙터 연결)")]
        [SerializeField] private GameObject markerPrefab;

        [Header("색상 (세기 i)")]
        [SerializeField] private Color colorStrong = new Color(0.2f, 0.9f, 0.3f, 0.85f); // 초록 (i=1)
        [SerializeField] private Color colorGentle = new Color(0.95f, 0.25f, 0.2f, 0.85f); // 빨강 (i=0)

        [Header("경로선 (Pathline)")]
        [SerializeField] private LineRenderer pathLineRenderer;

        [Header("방법별 파티클 프리팹 (m=1,2,3)")]
        [SerializeField] private GameObject pressEffectPrefab;  // m=1 누르기
        [SerializeField] private GameObject rubEffectPrefab;    // m=2 문지르기
        [SerializeField] private GameObject tapEffectPrefab;    // m=3 두드리기

        private readonly List<GameObject> _spawnedMarkers = new List<GameObject>();
        private readonly List<Vector3> _markerPositions = new List<Vector3>();

        // ── Strategy: 방법(m) 별 렌더러 전략 ────────────────────────────
        private IMassageStrategy _pressStrategy;
        private IMassageStrategy _rubStrategy;
        private IMassageStrategy _tapStrategy;

        private void Awake()
        {
            // 🛡️ [자가 치료] LineRenderer 컴포넌트 자동 조달
            if (pathLineRenderer == null)
            {
                pathLineRenderer = GetComponent<LineRenderer>();
                if (pathLineRenderer == null)
                {
                    pathLineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }

            // 전략 클래스 장착 (누락 프리팹 자가 복원 기술 탑재)
            _pressStrategy = new PressStrategy(pressEffectPrefab);
            _rubStrategy = new RubStrategy(rubEffectPrefab);
            _tapStrategy = new TapStrategy(tapEffectPrefab);
        }

        /// <summary>
        /// 페이로드의 혈자리 배열을 받아 AR 마커와 경로선을 렌더링.
        /// </summary>
        public void Render(List<ReceivedPoint> points, List<Vector3> worldPositions)
        {
            ClearAll();

            for (int i = 0; i < points.Count && i < worldPositions.Count; i++)
            {
                var pt = points[i];
                var pos = worldPositions[i];

                // 1. 🛡️ [마커 프리팹 자가 복원] 
                // 프리팹 배선을 누락한 경우, 즉석에서 정교한 3D Sphere 마커를 생성하여 씬 붕괴 방지!
                GameObject marker = null;
                if (markerPrefab == null)
                {
                    marker = ARMeshHelper.CreateVisualMarker($"AcupointMarker_Mock_{pt.id}", Color.white, 0.04f, pos, true);
                }
                else
                {
                    marker = Instantiate(markerPrefab, pos, Quaternion.identity);
                }

                _spawnedMarkers.Add(marker);
                _markerPositions.Add(pos);

                // 2. [WBS 3.5] 세기(i) ➡️ 발광 컬러 반영
                ApplyIntensityColor(marker, pt.i);

                // 3. [WBS 3.5] 방법(m) ➡️ Strategy Pattern으로 애니메이션/파티클 작동
                IMassageStrategy strategy = pt.m switch
                {
                    1 => _pressStrategy,
                    2 => _rubStrategy,
                    3 => _tapStrategy,
                    _ => _pressStrategy
                };
                strategy.Execute(pos);
            }

            // 4. [WBS 3.5] 경로(Pathline) 빛의 선 렌더링
            RenderPathline(_markerPositions);

            // 5. [WBS 3.6] Fake Drop Shadow 투영
            foreach (var marker in _spawnedMarkers)
                ProjectFakeDropShadow(marker.transform);
        }

        // ── 세기(i) 색상 적용 ─────────────────────────────────────────────
        private void ApplyIntensityColor(GameObject marker, int intensity)
        {
            var renderer = marker.GetComponentInChildren<Renderer>();
            if (renderer == null) return;

            // 🛡️ [자가 치료 URP / Built-in 쉐이더 동적 탐색기]
            // URP 환경에서는 Standard 쉐이더가 분홍색(Magenta)으로 오염됩니다.
            // 따라서 런타임에 가장 적합한 쉐이더를 순차 검색하여 캐스팅합니다.
            Shader activeShader = Shader.Find("Universal Render Pipeline/Lit");
            if (activeShader == null) activeShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (activeShader == null) activeShader = Shader.Find("Standard");
            if (activeShader == null) activeShader = Shader.Find("Sprites/Default");
            if (activeShader == null) activeShader = Shader.Find("UI/Default");

            Material mat = new Material(activeShader);
            mat.color = intensity == 1 ? colorStrong : colorGentle;

            // Emission 광학 효과 활성화 (URP & Built-in 공통)
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", mat.color * 2.5f); // 세기를 좀더 밝혀 광도 업그레이드

            renderer.material = mat;
        }

        // ── 경로선 (스플라인 렌더링) ──────────────────────────────────────
        private void RenderPathline(List<Vector3> positions)
        {
            if (pathLineRenderer == null || positions.Count < 2) return;

            pathLineRenderer.positionCount = positions.Count;
            pathLineRenderer.SetPositions(positions.ToArray());

            // 빛나는 스플라인 효과 설정 (하늘색 글로우 연출)
            pathLineRenderer.startColor = new Color(0.0f, 0.75f, 1.0f, 0.95f);
            pathLineRenderer.endColor = new Color(0.3f, 0.9f, 1.0f, 0.2f);
            pathLineRenderer.widthMultiplier = 0.01f; // 1cm 두께로 더욱 명확하게 빛남
            
            // 라인 머티리얼이 없는 경우를 위한 디폴트 모던 쉐이더 장착
            if (pathLineRenderer.sharedMaterial == null)
            {
                pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }

        // ── [WBS 3.6] Fake Drop Shadow (가상 방향광 투영) ──────────────────
        private void ProjectFakeDropShadow(Transform markerTransform)
        {
            Vector3 shadowPos = markerTransform.position + Vector3.down * 0.002f;
            Debug.DrawLine(markerTransform.position, shadowPos,
                new Color(0f, 0f, 0f, 0.45f), float.MaxValue);
        }

        /// <summary>
        /// 경로선(Pathline)만 렌더링. 마커는 AcupointAnchor가 별도 처리.
        /// </summary>
        public void RenderPathlineOnly(List<Vector3> worldPositions)
        {
            ClearAll();
            _markerPositions.AddRange(worldPositions);
            RenderPathline(_markerPositions);
        }

        public void ClearAll()
        {
            foreach (var m in _spawnedMarkers) 
            {
                if (m != null) Destroy(m);
            }
            _spawnedMarkers.Clear();
            _markerPositions.Clear();
            if (pathLineRenderer != null) pathLineRenderer.positionCount = 0;
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  [WBS 3.5] Strategy Pattern: 마사지 방법별 자가복원형 이펙트
    // ════════════════════════════════════════════════════════════════
    public interface IMassageStrategy
    {
        void Execute(Vector3 position);
    }

    public class PressStrategy : IMassageStrategy
    {
        private readonly GameObject _prefab;
        public PressStrategy(GameObject prefab) => _prefab = prefab;

        public void Execute(Vector3 position)
        {
            Debug.Log($"🟢 [PressStrategy] 지압(Deep Press) 효과 at {position}");
            if (_prefab != null)
            {
                Object.Instantiate(_prefab, position, Quaternion.identity);
            }
            else
            {
                // 🛡️ [이펙트 자가복원] 프리팹이 없을 시 하늘색의 수축 지압 지시용 구체 스폰 후 자동 소멸
                var mockEffect = ARMeshHelper.CreateVisualMarker("PressEffect_Mock", Color.cyan, 0.02f, position, true);

                // 1.2초 뒤 자동 파괴
                Object.Destroy(mockEffect, 1.2f);
            }
        }
    }

    public class RubStrategy : IMassageStrategy
    {
        private readonly GameObject _prefab;
        public RubStrategy(GameObject prefab) => _prefab = prefab;

        public void Execute(Vector3 position)
        {
            Debug.Log($"🟡 [RubStrategy] 문지르기(Gentle Rub) 효과 at {position}");
            if (_prefab != null)
            {
                Object.Instantiate(_prefab, position, Quaternion.identity);
            }
            else
            {
                // 🛡️ [이펙트 자가복원] 프리팹이 없을 시 맑은 노란색의 꼬마 문지르기 궤도 구체 스폰 후 자동 소멸
                var mockEffect = ARMeshHelper.CreateVisualMarker("RubEffect_Mock", Color.yellow, 0.02f, position, true);

                Object.Destroy(mockEffect, 1.2f);
            }
        }
    }

    public class TapStrategy : IMassageStrategy
    {
        private readonly GameObject _prefab;
        public TapStrategy(GameObject prefab) => _prefab = prefab;

        public void Execute(Vector3 position)
        {
            Debug.Log($"🔴 [TapStrategy] 두드리기(Rhythmic Tap) 효과 at {position}");
            if (_prefab != null)
            {
                Object.Instantiate(_prefab, position, Quaternion.identity);
            }
            else
            {
                // 🛡️ [이펙트 자가복원] 프리팹이 없을 시 다이내믹한 핑크빛 노이즈 구체 스폰 후 자동 소멸
                var mockEffect = ARMeshHelper.CreateVisualMarker("TapEffect_Mock", new Color(1.0f, 0.2f, 0.6f), 0.025f, position, true);

                Object.Destroy(mockEffect, 1.2f);
            }
        }
    }
}
