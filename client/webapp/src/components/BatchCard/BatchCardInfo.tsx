import { Typography } from "@mui/material";
import { HomeWork as ShedIcon } from "@mui/icons-material";

interface Props {
    breed?: string | null;
    shed?: string | null;
}

export default function BatchCardInfo({ breed, shed }: Props) {
    return (
        <>
            {/* Shed/Location if available */}
            {shed && (
                <Typography
                    variant="body2"
                    color="text.secondary"
                    sx={{
                        mb: 2,
                        display: "flex",
                        alignItems: "center",
                        gap: 0.5,
                    }}
                >
                    <ShedIcon fontSize="small" />
                    {shed}
                </Typography>
            )}

            {/* Breed if available */}
            {breed && (
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    Breed: {breed}
                </Typography>
            )}
        </>
    );
}
