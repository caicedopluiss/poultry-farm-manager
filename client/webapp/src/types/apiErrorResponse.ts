import type FieldValidationError from "./fieldValidationError";

export default interface ApiErrorResponse {
    statusCode: number;
    message: string;
    validationErrors?: FieldValidationError[];
}
