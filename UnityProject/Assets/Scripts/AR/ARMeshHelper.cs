using UnityEngine;

namespace MSG.AR
{
    /// <summary>
    /// [WBS 3.5] ARMeshHelper
    /// Unity Physics 모듈(SphereCollider 등)이 없어도 안전하게 3D 구체 및 다이아몬드 마커를
    /// 렌더링할 수 있도록 런타임에 3D 메쉬를 절차적(Procedural)으로 생성하는 헬퍼 클래스.
    /// </summary>
    public static class ARMeshHelper
    {
        private static Mesh _sphereMesh;
        private static Mesh _octahedronMesh;

        /// <summary>
        /// 절차적 UV 구체 메쉬(직경 1.0)를 반환. (없으면 생성)
        /// </summary>
        public static Mesh GetOrCreateSphereMesh()
        {
            if (_sphereMesh != null) return _sphereMesh;

            _sphereMesh = new Mesh();
            _sphereMesh.name = "ProceduralSphere";

            int longitudeCount = 12;
            int latitudeCount = 6;
            
            // 버텍스 생성
            int vertexCount = (longitudeCount + 1) * (latitudeCount + 1);
            Vector3[] vertices = new Vector3[vertexCount];
            
            int v = 0;
            for (int lat = 0; lat <= latitudeCount; lat++)
            {
                float theta = lat * Mathf.PI / latitudeCount;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);
                
                for (int lon = 0; lon <= longitudeCount; lon++)
                {
                    float phi = lon * 2f * Mathf.PI / longitudeCount;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);
                    
                    // 반경 0.5f로 설정하여 직경이 1.0f가 되도록 함 (Unity 기본 Sphere 크기와 동일)
                    float x = cosPhi * sinTheta * 0.5f;
                    float y = cosTheta * 0.5f;
                    float z = sinPhi * sinTheta * 0.5f;
                    
                    vertices[v++] = new Vector3(x, y, z);
                }
            }
            
            // 인덱스(삼각형) 생성
            int triangleCount = longitudeCount * latitudeCount * 6;
            int[] triangles = new int[triangleCount];
            
            int t = 0;
            for (int lat = 0; lat < latitudeCount; lat++)
            {
                for (int lon = 0; lon < longitudeCount; lon++)
                {
                    int current = lat * (longitudeCount + 1) + lon;
                    int next = current + 1;
                    int bottom = current + longitudeCount + 1;
                    int bottomNext = bottom + 1;
                    
                    // 삼각형 1
                    triangles[t++] = current;
                    triangles[t++] = bottom;
                    triangles[t++] = next;
                    
                    // 삼각형 2
                    triangles[t++] = next;
                    triangles[t++] = bottom;
                    triangles[t++] = bottomNext;
                }
            }
            
            _sphereMesh.vertices = vertices;
            _sphereMesh.triangles = triangles;
            _sphereMesh.RecalculateNormals();
            _sphereMesh.RecalculateBounds();
            
            return _sphereMesh;
        }

        /// <summary>
        /// 절차적 다이아몬드(옥타헤드론) 메쉬를 반환. (없으면 생성)
        /// </summary>
        public static Mesh GetOrCreateOctahedronMesh()
        {
            if (_octahedronMesh != null) return _octahedronMesh;

            _octahedronMesh = new Mesh();
            _octahedronMesh.name = "ProceduralOctahedron";

            Vector3[] vertices = new Vector3[]
            {
                new Vector3(0f, 0.5f, 0f),   // 0: Top
                new Vector3(0f, -0.5f, 0f),  // 1: Bottom
                new Vector3(-0.5f, 0f, 0f),  // 2: Left
                new Vector3(0.5f, 0f, 0f),   // 3: Right
                new Vector3(0f, 0f, 0.5f),   // 4: Front
                new Vector3(0f, 0f, -0.5f)   // 5: Back
            };

            int[] triangles = new int[]
            {
                0, 4, 3, // Top-Front-Right
                0, 3, 5, // Top-Right-Back
                0, 5, 2, // Top-Back-Left
                0, 2, 4, // Top-Left-Front
                1, 3, 4, // Bottom-Right-Front
                1, 5, 3, // Bottom-Back-Right
                1, 2, 5, // Bottom-Left-Back
                1, 4, 2  // Bottom-Front-Left
            };

            _octahedronMesh.vertices = vertices;
            _octahedronMesh.triangles = triangles;
            _octahedronMesh.RecalculateNormals();
            _octahedronMesh.RecalculateBounds();

            return _octahedronMesh;
        }

        /// <summary>
        /// 콜라이더가 없는 순수 비주얼 마커 오브젝트 생성.
        /// </summary>
        public static GameObject CreateVisualMarker(string name, Color color, float scale, Vector3 position, bool useSphere = true)
        {
            GameObject go = new GameObject(name);
            go.transform.position = position;

            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = useSphere ? GetOrCreateSphereMesh() : GetOrCreateOctahedronMesh();

            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Standard");

            if (shader != null)
            {
                Material mat = new Material(shader);
                mat.color = color;
                if (shader.name.Contains("Lit") || shader.name.Contains("Standard"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", color * 1.8f);
                }
                renderer.sharedMaterial = mat;
            }
            
            go.transform.localScale = Vector3.one * scale;
            return go;
        }
        /// <summary>
        /// 에디터 및 모바일 빌드 환경에서 오류 없이 안전하게 기본 UI Font 객체를 반환.
        /// </summary>

        public static Font GetSafeFont()
        {
            Font font = null;

            // 1단계: Resources 폴더에서 복사해둔 LiberationSans 폰트 자산 직접 로드 (가장 중요)
            try
            {
                font = Resources.Load<Font>("LiberationSans");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[ARMeshHelper] Resources.Load('LiberationSans') 실패: " + ex.Message);
            }

            // 2단계: 유니티 빌트인 기본 폰트 로드 시도
            if (font == null)
            {
                try
                {
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                catch {}
            }
            if (font == null)
            {
                try
                {
                    font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch {}
            }

            // 3단계: 씬 내에 이미 로드된 다른 폰트 자원 탐색
            if (font == null)
            {
                try
                {
                    Font[] loaded = Resources.FindObjectsOfTypeAll<Font>();
                    if (loaded != null && loaded.Length > 0)
                    {
                        foreach (var f in loaded)
                        {
                            if (f != null && !string.IsNullOrEmpty(f.name))
                            {
                                font = f;
                                break;
                            }
                        }
                    }
                }
                catch {}
            }

            // 4단계: OS 시스템 폰트에서 동적 생성 시도
            if (font == null)
            {
                try
                {
                    font = Font.CreateDynamicFontFromOSFont("Arial", 14);
                }
                catch {}
            }

            return font;
        }
    }
}
