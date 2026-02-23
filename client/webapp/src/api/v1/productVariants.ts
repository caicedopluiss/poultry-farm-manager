import type { ProductVariant, NewProductVariant, UpdateProductVariant } from "@/types/inventory";
import type { Transaction } from "@/types/transaction";
import type { Vendor } from "@/types/vendor";
import apiClient from "@/api/client";

const url = "v1/product-variants";

interface GetProductVariantsResponse {
    productVariants: ProductVariant[];
}

export async function getProductVariants(): Promise<GetProductVariantsResponse> {
    const response: GetProductVariantsResponse = await apiClient.get(url);
    return response;
}

interface GetProductVariantsByProductIdResponse {
    productVariants: ProductVariant[];
}

export async function getProductVariantsByProductId(productId: string): Promise<GetProductVariantsByProductIdResponse> {
    const response: GetProductVariantsByProductIdResponse = await apiClient.get(`v1/products/${productId}/variants`);
    return response;
}

interface GetProductVariantByIdResponse {
    productVariant: ProductVariant | null;
}

export async function getProductVariantById(id: string): Promise<GetProductVariantByIdResponse> {
    const response: GetProductVariantByIdResponse = await apiClient.get(`${url}/${id}`);
    return response;
}

interface CreateProductVariantResponse {
    createdProductVariant: ProductVariant;
}

export async function createProductVariant(variantData: NewProductVariant): Promise<CreateProductVariantResponse> {
    const response: CreateProductVariantResponse = await apiClient.post(url, { newProductVariant: variantData });
    return response;
}

interface UpdateProductVariantResponse {
    productVariant: ProductVariant;
}

export async function updateProductVariant(
    id: string,
    variantData: UpdateProductVariant,
): Promise<UpdateProductVariantResponse> {
    const response: UpdateProductVariantResponse = await apiClient.put(`${url}/${id}`, {
        updateProductVariant: variantData,
    });
    return response;
}

interface GetProductVariantTransactionsResponse {
    transactions: Transaction[];
}

export async function getProductVariantTransactions(variantId: string): Promise<GetProductVariantTransactionsResponse> {
    const response: GetProductVariantTransactionsResponse = await apiClient.get(`${url}/${variantId}/transactions`);
    return response;
}

export interface VendorPricing {
    vendor: Vendor;
    lastUnitPrice: number;
    lastPurchaseDate: string;
    totalPurchases: number;
}

interface GetProductVariantPricingByVendorResponse {
    vendorPricings: VendorPricing[];
}

export async function getProductVariantPricingByVendor(
    variantId: string,
): Promise<GetProductVariantPricingByVendorResponse> {
    const response: GetProductVariantPricingByVendorResponse = await apiClient.get(
        `${url}/${variantId}/pricing-by-vendor`,
    );
    return response;
}
