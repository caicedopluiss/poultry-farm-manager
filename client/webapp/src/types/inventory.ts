// AssetState types
export interface AssetState {
    id: string;
    status: string;
    quantity: number;
    location?: string | null;
}

// Asset types
export interface Asset {
    id: string;
    name: string;
    description?: string | null;
    notes?: string | null;
    states?: AssetState[] | null;
}

export interface NewAsset {
    name: string;
    description?: string | null;
    initialQuantity: number;
    notes?: string | null;
}

export interface UpdateAsset {
    name?: string | null;
    description?: string | null;
    notes?: string | null;
    states?: AssetState[] | null;
}

// Product types
export interface Product {
    id: string;
    name: string;
    manufacturer: string;
    unitOfMeasure: string;
    stock: number;
    description?: string | null;
    variants?: ProductVariant[] | null;
}

export interface NewProduct {
    name: string;
    manufacturer: string;
    unitOfMeasure: string;
    stock: number;
    description?: string | null;
}

export interface UpdateProduct {
    name?: string | null;
    manufacturer?: string | null;
    unitOfMeasure?: string | null;
    stock?: number | null;
    description?: string | null;
}

// Product Variant types
export interface ProductVariant {
    id: string;
    productId: string;
    name: string;
    unitOfMeasure: string;
    stock: number;
    quantity: number;
    description?: string | null;
    product?: Product | null;
}

export interface NewProductVariant {
    productId: string;
    name: string;
    unitOfMeasure: string;
    stock: number;
    quantity: number;
    description?: string | null;
}

export interface UpdateProductVariant {
    name?: string | null;
    unitOfMeasure?: string | null;
    stock?: number | null;
    quantity?: number | null;
    description?: string | null;
}

// Enums as const objects
export const AssetStatus = {
    Available: "Available",
    InUse: "InUse",
    Damaged: "Damaged",
    UnderMaintenance: "UnderMaintenance",
    Obsolete: "Obsolete",
    Disposed: "Disposed",
    Sold: "Sold",
    Leased: "Leased",
    Lost: "Lost",
} as const;

export type AssetStatusType = (typeof AssetStatus)[keyof typeof AssetStatus];

export const UnitOfMeasure = {
    Kilogram: "Kilogram",
    Gram: "Gram",
    Pound: "Pound",
    Liter: "Liter",
    Milliliter: "Milliliter",
    Gallon: "Gallon",
    Unit: "Unit",
    Piece: "Piece",
} as const;

export type UnitOfMeasureType = (typeof UnitOfMeasure)[keyof typeof UnitOfMeasure];
