import axios from "axios";
import type ApiErrorResponse from "@/types/apiErrorResponse";

export const API_RESULT_CODE_NO_RESPONSE: string = "NO_RESPONSE";
export const API_RESULT_CODE_BAD_REQUEST: string = "BAD_REQUEST";
export const API_RESULT_CODE_NOT_FOUND: string = "NOT_FOUND";

export interface ApiClientError {
    code: typeof API_RESULT_CODE_NO_RESPONSE | typeof API_RESULT_CODE_BAD_REQUEST | typeof API_RESULT_CODE_NOT_FOUND;
    response: ApiErrorResponse | null;
}

const apiClient = axios.create({
    baseURL: `${import.meta.env.VITE_API_HOST_URL}/api`,
    headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
    },
});

apiClient.interceptors.response.use(
    (response) => Promise.resolve(response.data),
    (error): Promise<ApiClientError> => {
        console.error("API error:", error);

        if (!error.response) {
            return Promise.reject({
                code: API_RESULT_CODE_NO_RESPONSE,
            });
        }

        if (error.response.status === 400) {
            return Promise.reject({
                code: API_RESULT_CODE_BAD_REQUEST,
                response: {
                    ...(!error.response.data ? { statusCode: 400, message: "Bad request." } : error.response.data),
                },
            });
        }

        if (error.response.status === 404) {
            return Promise.reject({
                code: API_RESULT_CODE_NOT_FOUND,
                response: {
                    ...(!error.response.data ? { statusCode: 404, message: "Not found." } : error.response.data),
                },
            });
        }

        return Promise.reject(error);
    }
);

export default apiClient;
