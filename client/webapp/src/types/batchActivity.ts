export type Sex = "Unsexed" | "Male" | "Female";

export type BatchActivityType = "MortalityRecording" | "Feeding";

export interface MortalityRegistration {
    id: string;
    batchId: string;
    type: string;
    numberOfDeaths: number;
    date: string;
    sex: Sex;
    notes?: string | null;
}

export interface NewMortalityRegistration {
    numberOfDeaths: number;
    dateClientIsoString: string;
    sex: Sex;
    notes?: string | null;
}

// Future activity types can be added here
// export interface FeedingActivity { ... }
