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
}

export default function BatchCardStats({ startDate, population, initialPopulation }: Props) {
    const calculateDays = (startDate: string): number => {
        const start = moment(startDate);
        const now = moment();
        return now.diff(start, "days");
    };

    const calculateWeeks = (startDate: string): number => {
        const start = moment(startDate);
        const now = moment();
        return now.diff(start, "weeks");
    };

    const calculateMortality = (initial: number, current: number): number => {
        if (initial === 0) return 0;
        return Math.round(((initial - current) / initial) * 100);
    };

    const days = calculateDays(startDate);
    const weeks = calculateWeeks(startDate);
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
