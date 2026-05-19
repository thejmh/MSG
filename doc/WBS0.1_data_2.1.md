# WBS 2.1 ~ 2.3 — Data Bridge (Handoff Layer)
**Layer:** Data | **Task ID:** 2.1 ~ 2.3 | **완료일:** 2026-05-17

---

## Task 2.1 — 페이로드 Minification
**파일:** `Scripts/Services/HandoffService.cs` → `BuildPayload()`

| 항목 | 결과 |
|------|------|
| 최소 스키마 (`dId`, `pts:[{id, i, m}]`) 조립 | ✅ |
| X/Y/Z 좌표 등 불필요한 메타데이터 완벽 배제 | ✅ |

**페이로드 스키마:**
```json
{
  "dId": "res_lower_back",
  "pts": [
    { "id": 148, "i": 1, "m": 1 },
    { "id": 165, "i": 1, "m": 1 }
  ]
}
```

---

## Task 2.2 — Base64URL 직렬화
**파일:** `Scripts/Services/HandoffService.cs` → `SerializeToBase64()` / `DeserializeFromBase64()`

| 항목 | 결과 |
|------|------|
| RFC 4648 Base64URL 인코딩 | ✅ `+→-`, `/→_`, `=` 제거 |
| 한글/특수문자 손실 없음 | ✅ UTF-8 바이트 변환 후 Base64 적용 |

---

## Task 2.3 — AR 가이드 시작 트리거
**파일:** `Scripts/UI/DumbComponents.cs` → `ResultCardUI.launchARButton`

| 항목 | 결과 |
|------|------|
| "✨ AR 가이드 시작" 버튼 바인딩 | ✅ `HandoffService.Instance.Handoff()` |
| Unity 단일 앱: SceneManager 전환 | ✅ `SceneManager.LoadScene("ARScene")` |
| 레거시 딥링크 호환 | ✅ `LaunchViaDeepLink()` 메서드 유지 |

**데이터 흐름:**
```
ResultCardUI 버튼 클릭
  → HandoffService.BuildPayload()   (Minification)
  → HandoffData.Instance.payload 저장
  → SceneManager.LoadScene("ARScene")
  → DeepLinkReceiver.ProcessPayload()  (AR 씬 수신)
```
