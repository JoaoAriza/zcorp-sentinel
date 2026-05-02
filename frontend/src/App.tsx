import { useEffect, useState } from "react";
import {
  AlertTriangle,
  ShieldCheck,
  Activity,
  Radio,
  UserCheck,
  Plus,
  X,
  LogOut,
} from "lucide-react";
import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
} from "recharts";
import { api } from "./services/api";
import { createIncidentConnection } from "./services/realtime";
import "./App.css";

type User = {
  userId: string;
  name: string;
  email: string;
  role: string;
};

type RecentRiskCase = {
  id: string;
  source: string;
  channel: string;
  subject: string;
  riskScore: number;
  classification: string;
  detectedSignals: string[];
  riskReasons: string[];
  createdAt: string;
};

type DashboardSummary = {
  totalCases: number;
  lowCases: number;
  mediumCases: number;
  highCases: number;
  criticalCases: number;
  averageRiskScore: number;
  casesByChannel: Record<string, number>;
  recentCases: RecentRiskCase[];
};

type LoginForm = {
  email: string;
  password: string;
};

type NewIncidentForm = {
  source: string;
  channel: string;
  subject: string;
  detectedSignals: string[];
};

const availableSignals = [
  "voice_clone",
  "face_mismatch",
  "executive_impersonation",
  "synthetic_identity",
  "behavior_anomaly",
  "urgent_language",
];

const initialForm: NewIncidentForm = {
  source: "",
  channel: "voice",
  subject: "",
  detectedSignals: [],
};

function App() {
  const [authLoading, setAuthLoading] = useState(true);
  const [user, setUser] = useState<User | null>(null);

  const [loginForm, setLoginForm] = useState<LoginForm>({
    email: "joao@zcorp.dev",
    password: "Sentinel@123",
  });

  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedCase, setSelectedCase] = useState<RecentRiskCase | null>(null);
  const [form, setForm] = useState<NewIncidentForm>(initialForm);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [search, setSearch] = useState("");
  const [channelFilter, setChannelFilter] = useState("all");
  const [severityFilter, setSeverityFilter] = useState("all");

  const [lastTotalCases, setLastTotalCases] = useState<number | null>(null);
  const [notification, setNotification] = useState<string | null>(null);
  const [authError, setAuthError] = useState("");

  async function loadDashboard(showNotification = false) {
    const response = await api.get<DashboardSummary>("/dashboard/summary");
    const nextSummary = response.data;

    setSummary((currentSummary) => {
      const previousTotal = currentSummary?.totalCases ?? lastTotalCases;

      if (
        showNotification &&
        previousTotal !== null &&
        nextSummary.totalCases > previousTotal
      ) {
        setNotification("New identity risk incident detected");

        setTimeout(() => {
          setNotification(null);
        }, 3200);
      }

      return nextSummary;
    });

    setLastTotalCases(nextSummary.totalCases);
  }

  async function loadMe() {
    const token = localStorage.getItem("zcorp_token");

    if (!token) {
      setAuthLoading(false);
      return;
    }

    try {
      const response = await api.get<User>("/auth/me");
      setUser(response.data);
      await loadDashboard();
    } catch {
      localStorage.removeItem("zcorp_token");
      localStorage.removeItem("zcorp_refresh_token");
      setUser(null);
      setSummary(null);
    } finally {
      setAuthLoading(false);
    }
  }

  async function login() {
    try {
      setAuthError("");

      const response = await api.post("/auth/login", loginForm);

      localStorage.setItem("zcorp_token", response.data.token);
      localStorage.setItem("zcorp_refresh_token", response.data.refreshToken);

      setUser({
        userId: response.data.userId,
        name: response.data.name,
        email: response.data.email,
        role: response.data.role,
      });

      await loadDashboard();
    } catch {
      setAuthError("Invalid credentials or unavailable API.");
    }
  }

  function logout() {
    localStorage.removeItem("zcorp_token");
    localStorage.removeItem("zcorp_refresh_token");
    setUser(null);
    setSummary(null);
  }

  async function createIncident() {
    if (!form.source.trim() || !form.subject.trim()) return;

    setIsSubmitting(true);

    try {
      await api.post("/identity-risk-cases", form);

      setForm(initialForm);
      setIsModalOpen(false);
      await loadDashboard();
    } finally {
      setIsSubmitting(false);
    }
  }

  function toggleSignal(signal: string) {
    setForm((current) => ({
      ...current,
      detectedSignals: current.detectedSignals.includes(signal)
        ? current.detectedSignals.filter((item) => item !== signal)
        : [...current.detectedSignals, signal],
    }));
  }

  useEffect(() => {
    loadMe();
  }, []);

  useEffect(() => {
    if (!user) return;

    const intervalId = window.setInterval(() => {
      loadDashboard(true);
    }, 5000);

    return () => {
      window.clearInterval(intervalId);
    };
  }, [user]);

  useEffect(() => {
    if (!user) return;

    const connection = createIncidentConnection(async () => {
      setNotification("Real-time incident received");
      await loadDashboard();

      setTimeout(() => {
        setNotification(null);
      }, 3200);
    });

    connection.start().catch(() => {
      console.error("SignalR connection failed.");
    });

    return () => {
      connection.stop();
    };
  }, [user]);

  if (authLoading) {
    return <div className="loading">Checking secure session...</div>;
  }

  if (!user) {
    return (
      <main className="login-page">
        <section className="login-card">
          <p className="eyebrow">ZCorp Sentinel Authenticity</p>
          <h1>Secure Access</h1>
          <p className="subtitle">
            Authenticate to access the deepfake and impersonation defense command center.
          </p>

          <div className="login-form">
            <input
              value={loginForm.email}
              placeholder="Email"
              onChange={(event) =>
                setLoginForm({ ...loginForm, email: event.target.value })
              }
            />

            <input
              value={loginForm.password}
              type="password"
              placeholder="Password"
              onChange={(event) =>
                setLoginForm({ ...loginForm, password: event.target.value })
              }
            />

            {authError && <div className="auth-error">{authError}</div>}

            <button className="primary-action" onClick={login}>
              Sign in
            </button>
          </div>
        </section>
      </main>
    );
  }

  if (!summary) {
    return <div className="loading">Loading Sentinel dashboard...</div>;
  }

  const severityData = [
    { name: "Low", value: summary.lowCases },
    { name: "Medium", value: summary.mediumCases },
    { name: "High", value: summary.highCases },
    { name: "Critical", value: summary.criticalCases },
  ];

  const channelData = Object.entries(summary.casesByChannel).map(([name, value]) => ({
    name,
    value,
  }));

  const filteredCases = summary.recentCases.filter((riskCase) => {
    const matchesSearch =
      riskCase.subject.toLowerCase().includes(search.toLowerCase()) ||
      riskCase.source.toLowerCase().includes(search.toLowerCase()) ||
      riskCase.channel.toLowerCase().includes(search.toLowerCase());

    const matchesChannel =
      channelFilter === "all" || riskCase.channel === channelFilter;

    const matchesSeverity =
      severityFilter === "all" || riskCase.classification === severityFilter;

    return matchesSearch && matchesChannel && matchesSeverity;
  });

  const severityColors: Record<string, string> = {
    Low: "#22c55e",
    Medium: "#eab308",
    High: "#f97316",
    Critical: "#f43f5e",
  };

  const channelColors: Record<string, string> = {
    voice: "#38bdf8",
    video: "#a78bfa",
    chat: "#22c55e",
    email: "#f97316",
  };

  const tooltipStyle = {
    background: "#0f172a",
    border: "1px solid rgba(148, 163, 184, 0.22)",
    borderRadius: "14px",
    color: "#e5eefb",
  };

  return (
    <main className="page">
      {notification && (
        <div className="toast">
          <div className="toast-pulse" />
          <span>{notification}</span>
        </div>
      )}

      <section className="hero">
        <div>
          <p className="eyebrow">ZCorp Sentinel Authenticity</p>
          <h1>Deepfake & Impersonation Defense Command Center</h1>
          <p className="subtitle">
            Monitor identity risk, synthetic signals, voice clone patterns and executive impersonation attempts in real time.
          </p>
        </div>

        <div className="hero-actions">
          <button className="primary-action" onClick={() => setIsModalOpen(true)}>
            <Plus size={18} />
            New Incident
          </button>

          <div className="status-card">
            <div className="status-pulse" />
            <span>{user.name} · {user.role}</span>
          </div>

          <button className="secondary-action" onClick={logout}>
            <LogOut size={16} />
            Logout
          </button>
        </div>
      </section>

      <section className="kpi-grid">
        <div className="kpi-card animated-card">
          <ShieldCheck />
          <span>Total Cases</span>
          <strong>{summary.totalCases}</strong>
        </div>

        <div className="kpi-card animated-card">
          <Activity />
          <span>Average Risk</span>
          <strong>{summary.averageRiskScore}</strong>
        </div>

        <div className="kpi-card danger animated-card">
          <AlertTriangle />
          <span>Critical</span>
          <strong>{summary.criticalCases}</strong>
        </div>

        <div className="kpi-card animated-card">
          <Radio />
          <span>High Risk</span>
          <strong>{summary.highCases}</strong>
        </div>
      </section>

      <section className="dashboard-grid">
        <div className="panel">
          <div className="panel-header">
            <h2>Risk Severity</h2>
            <span>classification distribution</span>
          </div>

          <div className="chart-block" onMouseDown={(event) => event.preventDefault()}>
            <ResponsiveContainer width="100%" height={260}>
              <PieChart accessibilityLayer={false}>
                <Pie
                  data={severityData.filter((item) => item.value > 0)}
                  dataKey="value"
                  nameKey="name"
                  innerRadius={56}
                  outerRadius={92}
                  paddingAngle={4}
                  stroke="rgba(15, 23, 42, 0.9)"
                  strokeWidth={4}
                  activeShape={false}
                >
                  {severityData
                    .filter((item) => item.value > 0)
                    .map((entry) => (
                      <Cell
                        key={entry.name}
                        fill={severityColors[entry.name] ?? "#38bdf8"}
                      />
                    ))}
                </Pie>
                <Tooltip
                  cursor={false}
                  contentStyle={tooltipStyle}
                  itemStyle={{ color: "#e5eefb" }}
                  labelStyle={{ color: "#93c5fd" }}
                />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="panel">
          <div className="panel-header">
            <h2>Cases by Channel</h2>
            <span>voice, video, chat and email</span>
          </div>

          <div className="chart-block" onMouseDown={(event) => event.preventDefault()}>
            <ResponsiveContainer width="100%" height={260}>
              <BarChart
                data={channelData}
                barCategoryGap="32%"
                accessibilityLayer={false}
              >
                <XAxis
                  dataKey="name"
                  axisLine={{ stroke: "rgba(148, 163, 184, 0.22)" }}
                  tickLine={false}
                  tick={{ fill: "#94a3b8", fontSize: 12 }}
                />
                <YAxis
                  allowDecimals={false}
                  axisLine={{ stroke: "rgba(148, 163, 184, 0.22)" }}
                  tickLine={false}
                  tick={{ fill: "#94a3b8", fontSize: 12 }}
                />
                <Tooltip
                  cursor={false}
                  contentStyle={tooltipStyle}
                  itemStyle={{ color: "#e5eefb" }}
                  labelStyle={{ color: "#93c5fd" }}
                />
                <Bar
                  dataKey="value"
                  radius={[12, 12, 4, 4]}
                  animationDuration={500}
                  activeBar={false}
                >
                  {channelData.map((entry) => (
                    <Cell
                      key={entry.name}
                      fill={channelColors[entry.name] ?? "#38bdf8"}
                    />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      </section>

      <section className="panel">
        <div className="panel-header">
          <h2>Recent Identity Risk Cases</h2>
          <span>latest flagged interactions</span>
        </div>

        <div className="filters">
          <input
            value={search}
            placeholder="Search by subject, source or channel..."
            onChange={(event) => setSearch(event.target.value)}
          />

          <select
            value={channelFilter}
            onChange={(event) => setChannelFilter(event.target.value)}
          >
            <option value="all">All channels</option>
            <option value="voice">Voice</option>
            <option value="video">Video</option>
            <option value="chat">Chat</option>
            <option value="email">Email</option>
          </select>

          <select
            value={severityFilter}
            onChange={(event) => setSeverityFilter(event.target.value)}
          >
            <option value="all">All severities</option>
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
            <option value="Critical">Critical</option>
          </select>
        </div>

        <div className="case-list">
          {filteredCases.map((riskCase) => (
            <div
              className="case-row clickable animated-row"
              key={riskCase.id}
              onClick={() => setSelectedCase(riskCase)}
            >
              <div className="case-icon">
                <UserCheck size={20} />
              </div>

              <div className="case-main">
                <strong>{riskCase.subject}</strong>
                <span>{riskCase.source} · {riskCase.channel}</span>
              </div>

              <div className={`badge ${riskCase.classification.toLowerCase()}`}>
                {riskCase.classification}
              </div>

              <div className="score">{riskCase.riskScore}</div>
            </div>
          ))}

          {filteredCases.length === 0 && (
            <div className="empty-state">
              No incidents found for the selected filters.
            </div>
          )}
        </div>
      </section>

      {isModalOpen && (
        <div className="modal-backdrop">
          <div className="modal">
            <div className="modal-header">
              <div>
                <p className="eyebrow">New Identity Risk Case</p>
                <h2>Create Incident</h2>
              </div>

              <button className="icon-button" onClick={() => setIsModalOpen(false)}>
                <X size={20} />
              </button>
            </div>

            <div className="form-grid">
              <label>
                Source
                <input
                  value={form.source}
                  placeholder="mobile-banking"
                  onChange={(event) =>
                    setForm({ ...form, source: event.target.value })
                  }
                />
              </label>

              <label>
                Channel
                <select
                  value={form.channel}
                  onChange={(event) =>
                    setForm({ ...form, channel: event.target.value })
                  }
                >
                  <option value="voice">Voice</option>
                  <option value="video">Video</option>
                  <option value="chat">Chat</option>
                  <option value="email">Email</option>
                </select>
              </label>

              <label className="full">
                Subject
                <input
                  value={form.subject}
                  placeholder="CEO approval call"
                  onChange={(event) =>
                    setForm({ ...form, subject: event.target.value })
                  }
                />
              </label>
            </div>

            <div className="signals">
              <span>Detected Signals</span>

              <div className="signal-grid">
                {availableSignals.map((signal) => (
                  <button
                    key={signal}
                    className={
                      form.detectedSignals.includes(signal)
                        ? "signal-chip active"
                        : "signal-chip"
                    }
                    onClick={() => toggleSignal(signal)}
                  >
                    {signal.replaceAll("_", " ")}
                  </button>
                ))}
              </div>
            </div>

            <div className="modal-actions">
              <button className="secondary-action" onClick={() => setIsModalOpen(false)}>
                Cancel
              </button>

              <button
                className="primary-action"
                onClick={createIncident}
                disabled={isSubmitting}
              >
                {isSubmitting ? "Creating..." : "Create Incident"}
              </button>
            </div>
          </div>
        </div>
      )}

      {selectedCase && (
        <div className="detail-backdrop" onClick={() => setSelectedCase(null)}>
          <div className="detail-drawer" onClick={(event) => event.stopPropagation()}>
            <div className="modal-header">
              <div>
                <p className="eyebrow">Incident Detail</p>
                <h2>{selectedCase.subject}</h2>
              </div>

              <button className="icon-button" onClick={() => setSelectedCase(null)}>
                <X size={20} />
              </button>
            </div>

            <div className="detail-grid">
              <div className="detail-card">
                <span>Source</span>
                <strong>{selectedCase.source}</strong>
              </div>

              <div className="detail-card">
                <span>Channel</span>
                <strong>{selectedCase.channel}</strong>
              </div>

              <div className="detail-card">
                <span>Risk Score</span>
                <strong>{selectedCase.riskScore}</strong>
              </div>

              <div className="detail-card">
                <span>Severity</span>
                <strong>{selectedCase.classification}</strong>
              </div>
            </div>

            <div className="analysis-box">
              <h3>Detected Signals</h3>

              <div className="reason-list">
                {selectedCase.detectedSignals?.map((signal) => (
                  <div className="reason-item" key={signal}>
                    {signal.replaceAll("_", " ")}
                  </div>
                ))}
              </div>
            </div>

            <div className="analysis-box">
              <h3>Risk Explanation</h3>

              <div className="reason-list">
                {selectedCase.riskReasons?.map((reason) => (
                  <div className="reason-item" key={reason}>
                    {reason}
                  </div>
                ))}
              </div>
            </div>

            <div className="detail-footer">
              <span>ID: {selectedCase.id}</span>
            </div>
          </div>
        </div>
      )}
    </main>
  );
}

export default App;