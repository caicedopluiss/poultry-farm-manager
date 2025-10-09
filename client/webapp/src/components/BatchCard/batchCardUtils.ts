export const getStatusCardColors = (status: string) => {
    switch (status.toLowerCase()) {
        case "planned":
            return {
                borderColor: "#e3f2fd", // Light blue
                backgroundColor: "#f8fdff",
                borderWidth: "2px",
            };
        case "active":
            return {
                borderColor: "#e8f5e8", // Light green
                backgroundColor: "#f9fff9",
                borderWidth: "2px",
            };
        case "forsale":
            return {
                borderColor: "#fff3e0", // Light orange
                backgroundColor: "#fffaf5",
                borderWidth: "2px",
            };
        case "completed":
            return {
                borderColor: "#f5f5f5", // Light gray
                backgroundColor: "#fafafa",
                borderWidth: "2px",
            };
        case "canceled":
            return {
                borderColor: "#ffebee", // Light red
                backgroundColor: "#fff8f8",
                borderWidth: "2px",
            };
        default:
            return {
                borderColor: "#f5f5f5",
                backgroundColor: "#fafafa",
                borderWidth: "1px",
            };
    }
};
