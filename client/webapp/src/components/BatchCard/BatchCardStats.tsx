import { Box } from "@mui/material";
import {
    Groups as PopulationIcon,
    TrendingDown as MortalityIcon,
    Schedule as DaysIcon,
    CalendarMonth as WeekIcon,
} from "@mui/icons-material";
import moment from "moment";
import BatchCardStatItem from "./BatchCardStatItem";

interface Props {
    startDate: string;
    population: number;
    initialPopulation: number;
    status: string;
    firstStatusChangeDate?: string | null;
}

export default function BatchCardStats({
    startDate,
    population,
    initialPopulation,
    status,
    firstStatusChangeDate,
}: Props) {
    const calculateDays = (startDate: string, status: string, firstStatusChangeDate?: string | null): number => {
        // Only continue counting for Active status
        // For other statuses, use the first status change date as the end date
        const shouldContinueCounting = !status || status.toLowerCase() === "active";
        const end = shouldContinueCounting || !firstStatusChangeDate ? moment() : moment(firstStatusChangeDate);
        // Add 1 to show current day (Day 1 on first day, not Day 0)
        return end.diff(moment(startDate), "days") + 1;
    };

    const calculateWeeks = (startDate: string, status: string, firstStatusChangeDate?: string | null): number => {
        // Only continue counting for Active status
        // For other statuses, use the first status change date as the end date
        const shouldContinueCounting = !status || status.toLowerCase() === "active";
        const end = shouldContinueCounting || !firstStatusChangeDate ? moment() : moment(firstStatusChangeDate);
        // Add 1 to show current week (Week 1 on first week, not Week 0)
        return end.diff(moment(startDate), "weeks") + 1;
    };

    const calculateMortality = (initial: number, current: number): number => {
        if (initial === 0) return 0;
        return Math.round(((initial - current) / initial) * 100);
    };

    const days = calculateDays(startDate, status, firstStatusChangeDate);
    const weeks = calculateWeeks(startDate, status, firstStatusChangeDate);
    const mortalityPercent = calculateMortality(initialPopulation, population);

    return (
        <Box
            sx={{
                mt: "auto",
                display: "grid",
                gridTemplateColumns: "1fr 1fr",
                gap: 2,
            }}
        >
            <BatchCardStatItem icon={<WeekIcon color="action" fontSize="small" />} label="Week" value={weeks} />

            <BatchCardStatItem icon={<DaysIcon color="action" fontSize="small" />} label="Days" value={days} />

            <BatchCardStatItem
                icon={<PopulationIcon color="action" fontSize="small" />}
                label="Population"
                value={population.toLocaleString()}
            />

            <BatchCardStatItem
                icon={<MortalityIcon fontSize="small" />}
                iconColor={mortalityPercent > 10 ? "error" : "action"}
                label="Mortality"
                value={`${mortalityPercent}%`}
                color={mortalityPercent > 10 ? "error.main" : undefined}
            />
        </Box>
    );
}
