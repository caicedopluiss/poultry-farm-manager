export interface FeedingTableDayEntry {
    id: string;
    dayNumber: number;
    foodType: string; // "PreInicio" | "Inicio" | "Engorde"
    amountPerBird: number;
    unitOfMeasure: string; // "Kilogram" | "Gram" | "Pound"
    expectedBirdWeight?: number | null;
    expectedBirdWeightUnitOfMeasure?: string | null;
}

export interface FeedingTable {
    id: string;
    name: string;
    description: string | null;
    dayEntries: FeedingTableDayEntry[];
}

export interface NewFeedingTableDayEntry {
    dayNumber: number;
    foodType: string;
    amountPerBird: number;
    unitOfMeasure: string;
    expectedBirdWeight?: number | null;
    expectedBirdWeightUnitOfMeasure?: string | null;
}

export interface NewFeedingTable {
    name: string;
    description?: string | null;
    dayEntries: NewFeedingTableDayEntry[];
}

export interface UpdateFeedingTable {
    name?: string | null;
    description?: string | null;
    dayEntries?: NewFeedingTableDayEntry[] | null;
}
