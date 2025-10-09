import { Box, Typography, Chip, useTheme, useMediaQuery } from "@mui/material";
import { Badge as StatusIcon } from "@mui/icons-material";

interface Props {
    name: string;
    status: string;
}

const getStatusColor = (status: string): "success" | "info" | "default" | "error" | "warning" => {
    switch (status.toLowerCase()) {
        case "active":
            return "success";
        case "planned":
            return "info";
        case "forsale":
            return "warning";
        case "completed":
            return "default";
        case "canceled":
            return "error";
        default:
            return "default";
    }
};

export default function BatchCardHeader({ name, status }: Props) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

    return (
        <Box
            sx={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "flex-start",
                mb: 2,
            }}
        >
            <Typography
                variant={isMobile ? "h6" : "h5"}
                component="h3"
                fontWeight="bold"
                sx={{
                    color: theme.palette.primary.main,
                    flex: 1,
                    mr: 1,
                }}
            >
                {name}
            </Typography>
            <Chip
                label={status}
                color={getStatusColor(status)}
                size="small"
                icon={<StatusIcon />}
                sx={{ fontWeight: "medium" }}
            />
        </Box>
    );
}
