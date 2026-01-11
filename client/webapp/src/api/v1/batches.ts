import type { Batch, NewBatch } from "@/types/batch";
import type {
    BatchActivity,
    MortalityRegistration,
    NewMortalityRegistration,
    StatusSwitch,
    NewStatusSwitch,
    ProductConsumption,
    NewProductConsumption,
} from "@/types/batchActivity";
import apiClient from "@/api/client";

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
    activities: BatchActivity[];
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
    mortalityData: NewMortalityRegistration,
): Promise<RegisterMortalityResponse> {
    const response: RegisterMortalityResponse = await apiClient.post(`${url}/${batchId}/mortality`, {
        mortalityRegistration: mortalityData,
    });
    return response;
}

interface SwitchStatusResponse {
    statusSwitch: StatusSwitch;
}
export async function switchBatchStatus(batchId: string, statusData: NewStatusSwitch): Promise<SwitchStatusResponse> {
    const response: SwitchStatusResponse = await apiClient.post(`${url}/${batchId}/status`, {
        statusSwitch: statusData,
    });
    return response;
}

interface RegisterProductConsumptionResponse {
    productConsumption: ProductConsumption;
}
export async function registerProductConsumption(
    batchId: string,
    consumptionData: NewProductConsumption,
): Promise<RegisterProductConsumptionResponse> {
    const response: RegisterProductConsumptionResponse = await apiClient.post(`${url}/${batchId}/product-consumption`, {
        productConsumption: consumptionData,
    });
    return response;
}

interface UpdateBatchNameResponse {
    batch: Batch;
}
export async function updateBatchName(batchId: string, name: string): Promise<UpdateBatchNameResponse> {
    const response: UpdateBatchNameResponse = await apiClient.put(`${url}/${batchId}/name`, {
        name,
    });
    return response;
}
