import type { Asset, NewAsset, UpdateAsset } from "@/types/inventory";
import apiClient from "@/api/client";

const url = "v1/assets";

interface GetAssetsResponse {
    assets: Asset[];
}

export async function getAssets(): Promise<GetAssetsResponse> {
    const response: GetAssetsResponse = await apiClient.get(url);
    return response;
}

interface GetAssetByIdResponse {
    asset: Asset | null;
}

export async function getAssetById(id: string): Promise<GetAssetByIdResponse> {
    const response: GetAssetByIdResponse = await apiClient.get(`${url}/${id}`);
    return response;
}

interface CreateAssetResponse {
    asset: Asset;
}

export async function createAsset(assetData: NewAsset): Promise<CreateAssetResponse> {
    const response: CreateAssetResponse = await apiClient.post(url, { newAsset: assetData });
    return response;
}

interface UpdateAssetResponse {
    asset: Asset;
}

export async function updateAsset(id: string, assetData: UpdateAsset): Promise<UpdateAssetResponse> {
    const response: UpdateAssetResponse = await apiClient.put(`${url}/${id}`, { updateAsset: assetData });
    return response;
}
