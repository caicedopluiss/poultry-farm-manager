export interface Person {
    id: string;
    firstName: string;
    lastName: string;
    email?: string;
    phoneNumber?: string;
    location?: string;
}

export interface NewPerson {
    firstName: string;
    lastName: string;
    email?: string;
    phoneNumber?: string;
    location?: string;
}

export interface UpdatePerson {
    firstName?: string;
    lastName?: string;
    email?: string;
    phoneNumber?: string;
    location?: string;
}
