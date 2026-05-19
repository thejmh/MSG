# WBS 0.1 — MSG 문진트리 JSON 정규화
**Layer:** Data | **Task ID:** 0.1 | **완료일:** 2026-05-17

## ✅ 완료 기준(DoD) 달성 여부
| 항목 | 결과 |
|------|------|
| Unity `Assets/Resources/` 배치 완료 | ✅ |
| DB 서버(API) 조회가 아님 확인 | ✅ 정적 JSON 파일, 외부 통신 없음 |

## 📄 생성 파일
`UnityProject/Assets/Resources/decision-tree.json`

## 🌳 트리 구조 (DAG 요약)
```
q_start (7개 부위)
├── q_head → q_head_pain → res_frontal/temporal/occipital/vertex_headache
│                       → res_eye_fatigue
│                       → res_dizziness
│                       → res_rhinitis
├── q_neck → res_neck_stiffness
│          → q_shoulder → res_frozen_shoulder / res_shoulder_fatigue
│          → res_throat
├── q_chest → res_cough / res_dyspnea / res_palpitation
├── q_digestive → res_indigestion / res_diarrhea / res_constipation / res_nausea
├── q_back → res_lower_back / res_upper_back / res_sciatica
├── q_arm → res_wrist_pain / res_arm_neuralgia / res_cold_hands
└── q_leg → res_knee_pain / res_calf_cramp / res_ankle_pain / res_leg_fatigue
```

## 🔑 스키마 (노드 단위)
```json
{
  "id": "q_head",
  "text": "어떤 증상이 있으신가요?",
  "isResult": false,
  "options": [
    { "text": "두통 (욱신욱신 아픔)", "nextId": "q_head_pain" }
  ]
}
```

- **질문 노드:** 10개 (id 접두사: `q_`)
- **결과 리프 노드:** 26개 (id 접두사: `res_`)
- **총 옵션(엣지):** 32개
