export type Sex = "Unsexed" | "Male" | "Female";

export type BatchActivityType = "MortalityRecording" | "StatusSwitch" | "Feeding" | "ProductConsumption";

export type BatchStatus = "Active" | "Processed" | "ForSale" | "Sold" | "Canceled";

// Base activity interface
export interface BatchActivity {
    id: string;
    batchId: string;
    type: BatchActivityType;
    date: string;
    notes?: string | null;
}

export interface MortalityRegistration extends BatchActivity {
    type: "MortalityRecording";
    numberOfDeaths: number;
    sex: Sex;
}

export interface NewMortalityRegistration {
    numberOfDeaths: number;
    dateClientIsoString: string;
    sex: Sex;
    notes?: string | null;
}

export interface StatusSwitch extends BatchActivity {
    type: "StatusSwitch";
    newStatus: BatchStatus;
}

export interface NewStatusSwitch {
    newStatus: BatchStatus;
    dateClientIsoString: string;
    notes?: string | null;
}

export interface ProductConsumption extends BatchActivity {
    type: "ProductConsumption";
    productId: string;
    productName: string;
    stock: number;
    unitOfMeasure: string;
}

export interface NewProductConsumption {
    productId: string;
    stock: number;
    unitOfMeasure: string;
    dateClientIsoString: string;
    notes?: string | null;
}

// Future activity types can be added here
// export interface FeedingActivity { ... }
