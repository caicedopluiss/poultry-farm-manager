export interface Batch {
    id: string;
    name: string;
    breed: string | null;
    status: string;
    startDate: string;
    initialPopulation: number;
    maleCount: number;
    femaleCount: number;
    unsexedCount: number;
    population: number;
    shed?: string | null; // Optional shed/location field
}
