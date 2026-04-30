import axios from "axios";

export const api = axios.create({
  baseURL: "http://localhost:5283/api",
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("zcorp_token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (
      error.response?.status === 401 &&
      !originalRequest._retry
    ) {
      originalRequest._retry = true;

      const refreshToken = localStorage.getItem("zcorp_refresh_token");

      if (!refreshToken) {
        localStorage.removeItem("zcorp_token");
        localStorage.removeItem("zcorp_refresh_token");
        return Promise.reject(error);
      }

      try {
        const response = await axios.post("http://localhost:5283/api/auth/refresh", {
          refreshToken,
        });

        localStorage.setItem("zcorp_token", response.data.token);
        localStorage.setItem("zcorp_refresh_token", response.data.refreshToken);

        originalRequest.headers.Authorization = `Bearer ${response.data.token}`;

        return api(originalRequest);
      } catch (refreshError) {
        localStorage.removeItem("zcorp_token");
        localStorage.removeItem("zcorp_refresh_token");
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);