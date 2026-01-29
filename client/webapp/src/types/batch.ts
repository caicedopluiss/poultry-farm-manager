export interface Batch {
    id: string;
    name: string;
    status: string;
    startDate: string;
    initialPopulation: number;
    maleCount: number;
    femaleCount: number;
    unsexedCount: number;
    population: number;
    breed?: string | null;
    shed?: string | null; // Optional shed/location field
    firstStatusChangeDate?: string | null;
}

export interface NewBatch {
    name: string;
    startClientDateIsoString: string;
    maleCount: number;
    femaleCount: number;
    unsexedCount: number;
    breed?: string | null;
    shed?: string | null;
    vendorId: string;
    initialCost: number;
}
