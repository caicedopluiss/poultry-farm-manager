import type { FeedingTable, NewFeedingTable, UpdateFeedingTable } from "@/types/feedingTable";
import apiClient from "@/api/client";

const url = "v1/feeding-tables";

interface GetFeedingTablesResponse {
    feedingTables: FeedingTable[];
}

export async function getFeedingTables(): Promise<GetFeedingTablesResponse> {
    const response: GetFeedingTablesResponse = await apiClient.get(url);
    return response;
}

interface GetFeedingTableByIdResponse {
    feedingTable: FeedingTable | null;
}

export async function getFeedingTableById(id: string): Promise<GetFeedingTableByIdResponse> {
    const response: GetFeedingTableByIdResponse = await apiClient.get(`${url}/${id}`);
    return response;
}

interface CreateFeedingTableResponse {
    feedingTable: FeedingTable;
}

export async function createFeedingTable(data: NewFeedingTable): Promise<CreateFeedingTableResponse> {
    const response: CreateFeedingTableResponse = await apiClient.post(url, { newFeedingTable: data });
    return response;
}

interface UpdateFeedingTableResponse {
    feedingTable: FeedingTable;
}

export async function updateFeedingTable(id: string, data: UpdateFeedingTable): Promise<UpdateFeedingTableResponse> {
    const response: UpdateFeedingTableResponse = await apiClient.patch(`${url}/${id}`, { updates: data });
    return response;
}
