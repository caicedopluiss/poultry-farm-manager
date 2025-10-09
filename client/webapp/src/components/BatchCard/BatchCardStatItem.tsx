import { Box, Typography } from "@mui/material";
import type { SvgIconProps } from "@mui/material";
import type { ReactElement } from "react";
import { cloneElement } from "react";

interface Props {
    icon: ReactElement<SvgIconProps>;
    label: string;
    value: string | number;
    color?: string;
    iconColor?: "inherit" | "action" | "disabled" | "primary" | "secondary" | "error" | "info" | "success" | "warning";
}

export default function BatchCardStatItem({ icon, label, value, color, iconColor = "action" }: Props) {
    return (
        <Box
            sx={{
                display: "flex",
                alignItems: "center",
                gap: 1,
            }}
        >
            {cloneElement(icon, { color: iconColor })}
            <Box>
                <Typography variant="body2" color="text.secondary">
                    {label}
                </Typography>
                <Typography variant="h6" fontWeight="bold" color={color || "text.primary"}>
                    {value}
                </Typography>
            </Box>
        </Box>
    );
}
