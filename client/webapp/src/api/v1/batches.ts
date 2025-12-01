import type { Batch, NewBatch } from "../../types/batch";
import type { MortalityRegistration, NewMortalityRegistration } from "../../types/batchActivity";
import apiClient from "../client";

const url = "v1/batches";

interface GetBatchesResponse {
    batches: Batch[];
}
export async function getBatches(): Promise<GetBatchesResponse> {
    const response: GetBatchesResponse = await apiClient.get(url);

    return response;
}

interface GetBatchByIdResponse {
    batch: Batch | null;
}
export async function getBatchById(id: string): Promise<GetBatchByIdResponse> {
    const response: GetBatchByIdResponse = await apiClient.get(`${url}/${id}`);

    return response;
}

interface PostBatchResponse {
    batch: Batch | null;
}
export async function postBatch(batchData: NewBatch): Promise<PostBatchResponse> {
    const response: PostBatchResponse = await apiClient.post(url, { newBatch: batchData });
    return response;
}

interface RegisterMortalityResponse {
    mortalityRegistration: MortalityRegistration;
}
export async function registerMortality(
    batchId: string,
    mortalityData: NewMortalityRegistration
): Promise<RegisterMortalityResponse> {
    const response: RegisterMortalityResponse = await apiClient.post(`${url}/${batchId}/mortality`, {
        mortalityRegistration: mortalityData,
    });
    return response;
}
