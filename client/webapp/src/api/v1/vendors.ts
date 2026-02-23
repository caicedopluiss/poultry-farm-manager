import type { Vendor, NewVendor, UpdateVendor } from "@/types/vendor";
import apiClient from "@/api/client";

const url = "v1/vendors";

interface GetVendorsResponse {
    vendors: Vendor[];
}

export async function getVendors(): Promise<GetVendorsResponse> {
    const response: GetVendorsResponse = await apiClient.get(url);
    return response;
}

interface GetVendorByIdResponse {
    vendor: Vendor | null;
}

export async function getVendorById(id: string): Promise<GetVendorByIdResponse> {
    const response: GetVendorByIdResponse = await apiClient.get(`${url}/${id}`);
    return response;
}

interface CreateVendorResponse {
    vendor: Vendor;
}

export async function createVendor(vendorData: NewVendor): Promise<CreateVendorResponse> {
    const response: CreateVendorResponse = await apiClient.post(url, { vendor: vendorData });
    return response;
}

interface UpdateVendorResponse {
    vendor: Vendor;
}

export async function updateVendor(id: string, vendorData: UpdateVendor): Promise<UpdateVendorResponse> {
    const response: UpdateVendorResponse = await apiClient.put(`${url}/${id}`, { vendor: vendorData });
    return response;
}
