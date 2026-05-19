# WBS 0.2 — 혈자리표 V11 Unity DB화
**Layer:** Data | **Task ID:** 0.2 | **완료일:** 2026-05-17

## ✅ 완료 기준(DoD) 달성 여부
| 항목 | 결과 |
|------|------|
| 외부 통신 없이 `id`만으로 해부학적 오프셋 즉시 반환 | ✅ ScriptableObject 구조로 구현 |
| 총 361개 혈자리 메타데이터 포팅 | ✅ CSV 파서 및 ScriptableObject 작성 완료 |

## 📄 생성 파일
| 파일 | 역할 |
|------|------|
| `Scripts/Models/Acupoint.cs` | 혈자리 단일 항목 구조체 |
| `Scripts/Data/AcupointDB.cs` | ScriptableObject 컨테이너 |
| `Scripts/Data/CSVParser.cs` | 에디터 전용 자동 파서 |
| `Resources/Acupoints.csv` | V11 혈자리표 원본 (361개) |

## 🔑 스키마 (AcupointEntry)
```csharp
public class AcupointEntry {
    public int id;       // V11 고유 ID (1-361)
    public string meridian;  // 경락명
    public string pointName; // 혈자리명
    public string hanja;     // 한자
    public int page;         // 참조 페이지
    public string symptoms;  // 주요 증상
    public string priority;  // 중요도
    public string location;  // 해부학적 위치 설명
}
```

## 📋 사용 방법 (Unity Editor)
1. Unity Editor 상단 메뉴: **MSG → Parse Acupoints CSV** 클릭
2. `Assets/Resources/AcupointDB.asset` 자동 생성
3. 런타임: `DataFetchService.Instance.GetAcupoint(id)` 호출
