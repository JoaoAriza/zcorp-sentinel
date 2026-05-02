# ZCorp Sentinel — Deepfake & Impersonation Defense Platform

## 🧠 Problem

Organizations are increasingly targeted by sophisticated social engineering attacks, including:

- Deepfake voice impersonation
- Executive fraud (CEO/CFO scams)
- Synthetic identities
- Behavioral anomalies across channels

Traditional systems generate alerts, but fail to answer a critical question:

> **Why is this incident risky?**

---

## 🚀 Solution

ZCorp Sentinel is an **explainable identity risk detection platform** that:

- Analyzes identity-related events
- Scores risk based on multi-signal heuristics
- Provides **human-readable explanations**
- Streams incidents in real time
- Maintains full audit traceability

---

## ⚙️ Core Features

### 🔐 Authentication System
- JWT + Refresh Token
- Role-based access (Admin / Analyst)
- Secure password hashing (BCrypt)

### 🧮 Risk Scoring Engine
- Multi-signal scoring logic
- Context-aware risk amplification
- Deterministic + explainable scoring

Example signals:
- `voice_clone`
- `executive_impersonation`
- `urgent_language`
- `behavior_anomaly`

---

### 🧾 Explainable Risk (Key Differentiator)

Each incident includes **risk reasoning**:

```json
"riskReasons": [
  "Voice channel increases impersonation risk (+25)",
  "Voice clone indicator detected (+30)",
  "Urgent language pattern detected (+15)"
]