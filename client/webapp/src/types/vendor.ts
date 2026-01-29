import type { Person } from "./person";

export interface Vendor {
    id: string;
    name: string;
    location?: string;
    contactPersonId: string;
    contactPerson?: Person;
}

export interface NewVendor {
    name: string;
    location?: string;
    contactPersonId: string;
}

export interface UpdateVendor {
    name?: string;
    location?: string;
    contactPersonId?: string;
}
