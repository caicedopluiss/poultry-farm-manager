import apiClient from "@/api/client";
import type { SaleOrder, NewSaleOrder, NewSaleOrderPayment } from "@/types/saleOrder";

const url = "v1/sale-orders";

interface GetBatchSaleOrdersResponse {
    saleOrders: SaleOrder[];
}

export async function getBatchSaleOrders(batchId: string): Promise<SaleOrder[]> {
    const response: GetBatchSaleOrdersResponse = await apiClient.get(`v1/batches/${batchId}/sale-orders`);
    return response.saleOrders;
}

interface SaleOrderResponse {
    saleOrder: SaleOrder;
}

export async function getSaleOrderById(id: string): Promise<SaleOrder | null> {
    const response: SaleOrderResponse = await apiClient.get(`${url}/${id}`);
    return response.saleOrder;
}

export async function createSaleOrder(newSaleOrder: NewSaleOrder): Promise<SaleOrder> {
    const response: SaleOrderResponse = await apiClient.post(url, { newSaleOrder });
    return response.saleOrder;
}

export async function addSaleOrderPayment(id: string, payment: NewSaleOrderPayment): Promise<SaleOrder> {
    const response: SaleOrderResponse = await apiClient.post(`${url}/${id}/payments`, { payment });
    return response.saleOrder;
}

export async function cancelSaleOrder(id: string): Promise<SaleOrder> {
    const response: SaleOrderResponse = await apiClient.post(`${url}/${id}/cancel`, {});
    return response.saleOrder;
}
