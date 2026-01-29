import type { Person, NewPerson, UpdatePerson } from "@/types/person";
import apiClient from "@/api/client";

const url = "v1/persons";

interface GetPersonsResponse {
    persons: Person[];
}

export async function getPersons(): Promise<GetPersonsResponse> {
    const response: GetPersonsResponse = await apiClient.get(url);
    return response;
}

interface GetPersonByIdResponse {
    person: Person | null;
}

export async function getPersonById(id: string): Promise<GetPersonByIdResponse> {
    const response: GetPersonByIdResponse = await apiClient.get(`${url}/${id}`);
    return response;
}

interface CreatePersonResponse {
    person: Person;
}

export async function createPerson(personData: NewPerson): Promise<CreatePersonResponse> {
    const response: CreatePersonResponse = await apiClient.post(url, { person: personData });
    return response;
}

interface UpdatePersonResponse {
    person: Person;
}

export async function updatePerson(id: string, personData: UpdatePerson): Promise<UpdatePersonResponse> {
    const response: UpdatePersonResponse = await apiClient.put(`${url}/${id}`, { person: personData });
    return response;
}
