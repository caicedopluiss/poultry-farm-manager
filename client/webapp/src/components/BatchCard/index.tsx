import { Card, CardContent, CardActionArea, useTheme } from "@mui/material";
import type { Batch } from "@/types/batch";
import BatchCardHeader from "./BatchCardHeader";
import BatchCardInfo from "./BatchCardInfo";
import BatchCardStats from "./BatchCardStats";
import { getStatusCardColors } from "./batchCardUtils";

interface Props {
    batch: Batch;
    onClick?: (batch: Batch) => void;
}

export default function BatchCard({ batch, onClick }: Props) {
    const theme = useTheme();
    const cardColors = getStatusCardColors(batch.status);

    const handleClick = () => {
        onClick?.(batch);
    };

    return (
        <Card
            sx={{
                height: "100%",
                border: `${cardColors.borderWidth} solid ${cardColors.borderColor}`,
                backgroundColor: cardColors.backgroundColor,
                transition: "all 0.2s ease-in-out",
                "&:hover": {
                    transform: "translateY(-2px)",
                    boxShadow: theme.shadows[4],
                },
            }}
        >
            <CardActionArea onClick={handleClick} sx={{ height: "100%", p: 0 }}>
                <CardContent
                    sx={{
                        height: "100%",
                        display: "flex",
                        flexDirection: "column",
                    }}
                >
                    <BatchCardHeader name={batch.name} status={batch.status} />
                    <BatchCardInfo breed={batch.breed} shed={batch.shed} />
                    <BatchCardStats
                        startDate={batch.startDate}
                        population={batch.population}
                        initialPopulation={batch.initialPopulation}
                        status={batch.status}
                        firstStatusChangeDate={batch.firstStatusChangeDate}
                    />
                </CardContent>
            </CardActionArea>
        </Card>
    );
}
