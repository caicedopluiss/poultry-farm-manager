import type { Product, NewProduct, UpdateProduct } from "@/types/inventory";
import apiClient from "@/api/client";

const url = "v1/products";

interface GetProductsResponse {
    products: Product[];
}

export async function getProducts(): Promise<GetProductsResponse> {
    const response: GetProductsResponse = await apiClient.get(url);
    return response;
}

interface GetProductByIdResponse {
    product: Product | null;
}

export async function getProductById(id: string): Promise<GetProductByIdResponse> {
    const response: GetProductByIdResponse = await apiClient.get(`${url}/${id}`);
    return response;
}

interface CreateProductResponse {
    product: Product;
}

export async function createProduct(productData: NewProduct): Promise<CreateProductResponse> {
    const response: CreateProductResponse = await apiClient.post(url, { newProduct: productData });
    return response;
}

interface UpdateProductResponse {
    product: Product;
}

export async function updateProduct(id: string, productData: UpdateProduct): Promise<UpdateProductResponse> {
    const response: UpdateProductResponse = await apiClient.put(`${url}/${id}`, { updateProduct: productData });
    return response;
}

interface AddProductStockResponse {
    updatedProduct: Product;
}

export async function addProductStock(
    productId: string,
    productVariantId: string,
    quantity: number,
): Promise<AddProductStockResponse> {
    const response: AddProductStockResponse = await apiClient.post(`${url}/${productId}/add-stock`, {
        productVariantId,
        quantity,
    });
    return response;
}
