import { useState } from "react";
import { alpha } from "@mui/material/styles";
import { useNavigate } from "react-router-dom";
import {
    Container,
    Typography,
    Paper,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    Divider,
    useTheme,
    useMediaQuery,
    Menu,
    MenuItem,
    ListItemIcon,
    ListItemText,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    FormControl,
    InputLabel,
    Select,
    TextField,
    Alert,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Tooltip,
} from "@mui/material";
import {
    ArrowBack as BackIcon,
    Groups as PopulationIcon,
    TrendingDown as MortalityIcon,
    Schedule as DaysIcon,
    Badge as StatusIcon,
    HomeWork as ShedIcon,
    CalendarMonth as WeekIcon,
    CalendarToday as StartDateIcon,
    Pets as BreedIcon,
    Add as AddIcon,
    LocalHospital as MortalityActivityIcon,
    SwapHoriz as StatusSwitchIcon,
    Inventory as ProductConsumptionIcon,
    Edit as EditIcon,
    Scale as WeightMeasurementIcon,
    AttachMoney as FinanceIcon,
    Notes as NotesIcon,
    SetMeal as FeedingTableIcon,
    LinkOff as UnlinkIcon,
} from "@mui/icons-material";
import moment from "moment";
import type { Batch } from "@/types/batch";
import type { FeedingTable, FeedingTableDayEntry } from "@/types/feedingTable";
import type {
    BatchActivity,
    StatusSwitch,
    MortalityRegistration,
    ProductConsumption,
    WeightMeasurement,
    BatchActivityType,
} from "@/types/batchActivity";
import RegisterActivityDialog from "@/components/RegisterActivityDialog";
import EditBatchNameDialog from "@/components/EditBatchNameDialog";
import EditBatchNotesDialog from "@/components/EditBatchNotesDialog";
import { getFeedingTables } from "@/api/v1/feedingTables";
import { assignFeedingTableToBatch, updateBatchDailyFeedingTimes } from "@/api/v1/batches";

interface BatchDetailProps {
    batch: Batch;
    activities?: BatchActivity[];
    onRefresh?: () => void;
}

export default function BatchDetail({ batch, activities = [], onRefresh }: BatchDetailProps) {
    const navigate = useNavigate();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    const [activityDialogOpen, setActivityDialogOpen] = useState(false);
    const [selectedActivityType, setSelectedActivityType] = useState<BatchActivityType | null>(null);
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const menuOpen = Boolean(anchorEl);
    const [editNameDialogOpen, setEditNameDialogOpen] = useState(false);
    const [editNotesDialogOpen, setEditNotesDialogOpen] = useState(false);

    // Feeding table assignment state
    const [assignDialogOpen, setAssignDialogOpen] = useState(false);
    const [availableFeedingTables, setAvailableFeedingTables] = useState<FeedingTable[]>([]);
    const [selectedFeedingTableId, setSelectedFeedingTableId] = useState<string>("");
    const [assigning, setAssigning] = useState(false);
    const [assignError, setAssignError] = useState<string | null>(null);

    // Daily feeding times edit state
    const [editFeedingTimesOpen, setEditFeedingTimesOpen] = useState(false);

    // Lifecycle feeding plan dialog state
    const [lifecyclePlanOpen, setLifecyclePlanOpen] = useState(false);
    const [editFeedingTimesValue, setEditFeedingTimesValue] = useState<string>("");
    const [savingFeedingTimes, setSavingFeedingTimes] = useState(false);
    const [editFeedingTimesError, setEditFeedingTimesError] = useState<string | null>(null);

    const calculateDays = (startDate: string, firstStatusChangeDate?: string | null, status?: string): number => {
        // Only continue counting for Active status
        const shouldContinueCounting = !status || status.toLowerCase() === "active";
        const end = shouldContinueCounting || !firstStatusChangeDate ? moment() : moment(firstStatusChangeDate);
        // Add 1 to show current day (Day 1 on first day, not Day 0)
        return end.diff(moment(startDate), "days") + 1;
    };

    const calculateWeeks = (startDate: string, firstStatusChangeDate?: string | null, status?: string): number => {
        // Only continue counting for Active status
        const shouldContinueCounting = !status || status.toLowerCase() === "active";
        const end = shouldContinueCounting || !firstStatusChangeDate ? moment() : moment(firstStatusChangeDate);
        // Add 1 to show current week (Week 1 on first week, not Week 0)
        return end.diff(moment(startDate), "weeks") + 1;
    };

    const calculateMortality = (initial: number, current: number): number => {
        if (initial === 0) return 0;
        return Math.round(((initial - current) / initial) * 100);
    };

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

    const canSwitchStatus = (): boolean => {
        const status = batch.status.toLowerCase();
        // Status transitions: Active -> Processed/ForSale/Canceled, Processed -> ForSale, ForSale -> Sold
        return status === "active" || status === "processed" || status === "forsale";
    };

    const canRegisterMortality = (): boolean => {
        // Can only register mortality for active batches
        return batch.status.toLowerCase() === "active";
    };

    const canRegisterWeightMeasurement = (): boolean => {
        // Can only register weight measurements for active batches
        return batch.status.toLowerCase() === "active";
    };

    const days = calculateDays(batch.startDate, batch.firstStatusChangeDate, batch.status);
    const weeks = calculateWeeks(batch.startDate, batch.firstStatusChangeDate, batch.status);
    const mortalityPercent = calculateMortality(batch.initialPopulation, batch.population);

    const effectiveDailyFeedingTimes =
        batch.dailyFeedingTimes !== null && batch.dailyFeedingTimes !== undefined && batch.dailyFeedingTimes > 0
            ? batch.dailyFeedingTimes
            : 1;

    const currentDayEntry =
        batch.feedingTable && batch.feedingTable.dayEntries && batch.feedingTable.dayEntries.length > 0
            ? (batch.feedingTable.dayEntries.find((e) => e.dayNumber === days) ??
              [...batch.feedingTable.dayEntries].sort((a, b) => b.dayNumber - a.dayNumber)[0])
            : null;
    const totalBatchAmountPerSession =
        currentDayEntry !== null
            ? (() => {
                  const raw = (currentDayEntry.amountPerBird * batch.population) / effectiveDailyFeedingTimes;
                  if (currentDayEntry.unitOfMeasure === "Kilogram") {
                      return Math.round(raw / 0.05) * 0.05;
                  }
                  return Math.round(raw * 1000) / 1000;
              })()
            : null;

    const formatTotal = (value: number, unitOfMeasure: string): string => {
        if (unitOfMeasure === "Kilogram") {
            return value.toFixed(2);
        }
        return String(value);
    };

    const sortedFeedingEntries: FeedingTableDayEntry[] = batch.feedingTable?.dayEntries
        ? [...batch.feedingTable.dayEntries].sort((a, b) => a.dayNumber - b.dayNumber)
        : [];

    const calcEntryTotalPerDay = (entry: FeedingTableDayEntry): number => {
        const raw = entry.amountPerBird * batch.population;
        if (entry.unitOfMeasure === "Kilogram") {
            return Math.round(raw / 0.05) * 0.05;
        }
        return Math.round(raw * 1000) / 1000;
    };

    const calcEntryPerSession = (entry: FeedingTableDayEntry): number => {
        const raw = calcEntryTotalPerDay(entry) / effectiveDailyFeedingTimes;
        if (entry.unitOfMeasure === "Kilogram") {
            return Math.round(raw / 0.05) * 0.05;
        }
        return Math.round(raw * 1000) / 1000;
    };

    const handleOpenActivityMenu = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleCloseActivityMenu = () => {
        setAnchorEl(null);
    };

    const handleSelectActivity = (activityType: BatchActivityType) => {
        setSelectedActivityType(activityType);
        setActivityDialogOpen(true);
        handleCloseActivityMenu();
    };

    const handleCloseActivityDialog = () => {
        setActivityDialogOpen(false);
        setSelectedActivityType(null);
    };

    const handleActivitySuccess = () => {
        // Refresh the batch data by calling the parent's refresh callback
        if (onRefresh) {
            onRefresh();
        }
    };

    const handleOpenAssignDialog = async () => {
        setAssignError(null);
        setSelectedFeedingTableId(batch.feedingTable?.id ?? "");
        try {
            const { feedingTables } = await getFeedingTables();
            setAvailableFeedingTables(feedingTables);
        } catch {
            setAvailableFeedingTables([]);
        }
        setAssignDialogOpen(true);
    };

    const handleConfirmAssign = async () => {
        if (!selectedFeedingTableId) return;
        setAssigning(true);
        setAssignError(null);
        try {
            await assignFeedingTableToBatch(batch.id, selectedFeedingTableId);
            setAssignDialogOpen(false);
            if (onRefresh) onRefresh();
        } catch (err: unknown) {
            const apiErr = err as { response?: { message?: string } };
            setAssignError(apiErr?.response?.message || "Failed to assign feeding table.");
        } finally {
            setAssigning(false);
        }
    };

    const handleUnassignFeedingTable = async () => {
        setAssigning(true);
        try {
            await assignFeedingTableToBatch(batch.id, null);
            if (onRefresh) onRefresh();
        } catch {
            // silently fail — user can retry
        } finally {
            setAssigning(false);
        }
    };

    const handleOpenEditFeedingTimes = () => {
        setEditFeedingTimesValue(batch.dailyFeedingTimes !== null ? String(batch.dailyFeedingTimes) : "");
        setEditFeedingTimesError(null);
        setEditFeedingTimesOpen(true);
    };

    const handleSaveFeedingTimes = async () => {
        const value = editFeedingTimesValue.trim() === "" ? null : parseInt(editFeedingTimesValue, 10);
        if (value !== null && (isNaN(value) || value < 1)) {
            setEditFeedingTimesError("Must be a positive integer (or leave empty to clear).");
            return;
        }
        setSavingFeedingTimes(true);
        setEditFeedingTimesError(null);
        try {
            await updateBatchDailyFeedingTimes(batch.id, value);
            setEditFeedingTimesOpen(false);
            if (onRefresh) onRefresh();
        } catch (err: unknown) {
            const apiErr = err as { response?: { message?: string } };
            setEditFeedingTimesError(apiErr?.response?.message || "Failed to update daily feeding times.");
        } finally {
            setSavingFeedingTimes(false);
        }
    };

    return (
        <Container maxWidth="lg" sx={{ py: 3 }}>
            {/* Header */}
            <Box sx={{ mb: 4 }}>
                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "flex-start",
                        mb: 2,
                        flexWrap: "wrap",
                        gap: 2,
                    }}
                >
                    <Button variant="outlined" startIcon={<BackIcon />} onClick={() => navigate("/")}>
                        Back to Batches
                    </Button>
                    <Button
                        variant="contained"
                        color="success"
                        startIcon={<FinanceIcon />}
                        onClick={() => navigate(`/batches/${batch.id}/finance`)}
                    >
                        View Finance
                    </Button>
                </Box>

                <Box sx={{ display: "flex", alignItems: "center", gap: 2, mb: 2, flexWrap: "wrap" }}>
                    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                        <Typography
                            variant={isMobile ? "h4" : "h3"}
                            component="h1"
                            fontWeight="bold"
                            sx={{ color: theme.palette.primary.main }}
                        >
                            {batch.name}
                        </Typography>
                        <Button
                            size="small"
                            variant="outlined"
                            startIcon={<EditIcon />}
                            onClick={() => setEditNameDialogOpen(true)}
                            sx={{ minWidth: "auto", px: 1.5 }}
                        >
                            {!isMobile && "Edit"}
                        </Button>
                    </Box>
                    <Chip
                        label={batch.status}
                        color={getStatusColor(batch.status)}
                        icon={<StatusIcon />}
                        sx={{ fontWeight: "medium", fontSize: "1rem" }}
                    />
                </Box>
            </Box>

            <Box
                sx={{
                    display: "grid",
                    gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" },
                    gap: 3,
                }}
            >
                {/* Basic Information */}
                <Card>
                    <CardContent>
                        <Typography variant="h6" component="div" gutterBottom fontWeight="bold">
                            Basic Information
                        </Typography>
                        <Divider sx={{ mb: 2 }} />

                        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                <StartDateIcon color="action" />
                                <Box>
                                    <Typography variant="body2" color="text.secondary">
                                        Start Date
                                    </Typography>
                                    <Typography variant="body1" fontWeight="medium">
                                        {moment(batch.startDate).format("MMMM DD, YYYY")}
                                    </Typography>
                                </Box>
                            </Box>

                            {batch.breed && (
                                <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                    <BreedIcon color="action" />
                                    <Box>
                                        <Typography variant="body2" color="text.secondary">
                                            Breed
                                        </Typography>
                                        <Typography variant="body1" fontWeight="medium">
                                            {batch.breed}
                                        </Typography>
                                    </Box>
                                </Box>
                            )}

                            {batch.shed && (
                                <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                    <ShedIcon color="action" />
                                    <Box>
                                        <Typography variant="body2" color="text.secondary">
                                            Location/Shed
                                        </Typography>
                                        <Typography variant="body1" fontWeight="medium">
                                            {batch.shed}
                                        </Typography>
                                    </Box>
                                </Box>
                            )}

                            <Box sx={{ mt: 2 }}>
                                <Box
                                    sx={{
                                        display: "flex",
                                        justifyContent: "space-between",
                                        alignItems: "center",
                                        mb: 1,
                                    }}
                                >
                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                        sx={{ display: "flex", alignItems: "center", gap: 0.5 }}
                                    >
                                        <NotesIcon fontSize="small" />
                                        Notes
                                    </Typography>
                                    <Button
                                        size="small"
                                        variant="outlined"
                                        startIcon={<EditIcon />}
                                        onClick={() => setEditNotesDialogOpen(true)}
                                        sx={{ minWidth: "auto", px: 1 }}
                                    >
                                        Edit
                                    </Button>
                                </Box>
                                <Paper
                                    variant="outlined"
                                    sx={{
                                        p: 2,
                                        minHeight: 80,
                                        bgcolor: "grey.50",
                                    }}
                                >
                                    <Typography variant="body2" sx={{ whiteSpace: "pre-wrap" }}>
                                        {batch.notes || "No notes added yet."}
                                    </Typography>
                                </Paper>
                            </Box>
                        </Box>
                    </CardContent>
                </Card>

                {/* Time Metrics */}
                <Card>
                    <CardContent>
                        <Typography variant="h6" component="div" gutterBottom fontWeight="bold">
                            Time Metrics
                        </Typography>
                        <Divider sx={{ mb: 2 }} />

                        <Box
                            sx={{
                                display: "grid",
                                gridTemplateColumns: "1fr 1fr",
                                gap: 2,
                            }}
                        >
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                <WeekIcon color="action" />
                                <Box>
                                    <Typography variant="body2" color="text.secondary">
                                        Week
                                    </Typography>
                                    <Typography variant="h5" fontWeight="bold">
                                        {weeks}
                                    </Typography>
                                </Box>
                            </Box>
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                <DaysIcon color="action" />
                                <Box>
                                    <Typography variant="body2" color="text.secondary">
                                        Days
                                    </Typography>
                                    <Typography variant="h5" fontWeight="bold">
                                        {days}
                                    </Typography>
                                </Box>
                            </Box>
                        </Box>
                    </CardContent>
                </Card>
            </Box>

            {/* Population Details */}
            <Card sx={{ mt: 3 }}>
                <CardContent>
                    <Typography variant="h6" component="div" gutterBottom fontWeight="bold">
                        Population Details
                    </Typography>
                    <Divider sx={{ mb: 3 }} />

                    <Box
                        sx={{
                            display: "grid",
                            gridTemplateColumns: {
                                xs: "1fr",
                                sm: "1fr 1fr",
                                md: "repeat(4, 1fr)",
                            },
                            gap: 3,
                            mb: 3,
                        }}
                    >
                        <Paper
                            elevation={2}
                            sx={{
                                p: 2,
                                textAlign: "center",
                                bgcolor: "primary.50",
                                border: "1px solid",
                                borderColor: "primary.200",
                            }}
                        >
                            <PopulationIcon sx={{ fontSize: 32, color: "primary.main", mb: 1 }} />
                            <Typography variant="body2" color="text.secondary">
                                Current Population
                            </Typography>
                            <Typography variant="h4" fontWeight="bold" color="primary.main">
                                {batch.population.toLocaleString()}
                            </Typography>
                        </Paper>

                        <Paper
                            elevation={2}
                            sx={{
                                p: 2,
                                textAlign: "center",
                                bgcolor: "grey.50",
                                border: "1px solid",
                                borderColor: "grey.300",
                            }}
                        >
                            <Typography variant="body2" color="text.secondary">
                                Initial Population
                            </Typography>
                            <Typography variant="h4" fontWeight="bold">
                                {batch.initialPopulation.toLocaleString()}
                            </Typography>
                        </Paper>

                        <Paper
                            elevation={2}
                            sx={{
                                p: 2,
                                textAlign: "center",
                                bgcolor: (theme) =>
                                    mortalityPercent > 10
                                        ? alpha(theme.palette.error.main, 0.08)
                                        : alpha(theme.palette.success.main, 0.08),
                                border: "1px solid",
                                borderColor: mortalityPercent > 10 ? "error.light" : "success.light",
                            }}
                        >
                            <MortalityIcon
                                sx={{
                                    fontSize: 32,
                                    color: mortalityPercent > 10 ? "error.main" : "success.main",
                                    mb: 1,
                                }}
                            />
                            <Typography variant="body2" color="text.secondary">
                                Mortality Rate
                            </Typography>
                            <Typography
                                variant="h4"
                                fontWeight="bold"
                                color={mortalityPercent > 10 ? "error.main" : "success.main"}
                            >
                                {mortalityPercent}%
                            </Typography>
                        </Paper>

                        <Paper
                            elevation={2}
                            sx={{
                                p: 2,
                                textAlign: "center",
                                bgcolor: "warning.50",
                                border: "1px solid",
                                borderColor: "warning.200",
                            }}
                        >
                            <Typography variant="body2" color="text.secondary">
                                Total Lost
                            </Typography>
                            <Typography variant="h4" fontWeight="bold" color="warning.main">
                                {(batch.initialPopulation - batch.population).toLocaleString()}
                            </Typography>
                        </Paper>
                    </Box>

                    {/* Population Breakdown */}
                    <Box>
                        <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                            Population Breakdown
                        </Typography>
                        <Box
                            sx={{
                                display: "grid",
                                gridTemplateColumns: "repeat(3, 1fr)",
                                gap: 2,
                            }}
                        >
                            <Box sx={{ textAlign: "center" }}>
                                <Typography variant="body2" color="text.secondary">
                                    Male
                                </Typography>
                                <Typography variant="h6" component="div" fontWeight="bold" color="info.main">
                                    {batch.maleCount.toLocaleString()}
                                </Typography>
                            </Box>
                            <Box sx={{ textAlign: "center" }}>
                                <Typography variant="body2" color="text.secondary">
                                    Female
                                </Typography>
                                <Typography variant="h6" component="div" fontWeight="bold" color="secondary.main">
                                    {batch.femaleCount.toLocaleString()}
                                </Typography>
                            </Box>
                            <Box sx={{ textAlign: "center" }}>
                                <Typography variant="body2" color="text.secondary">
                                    Unsexed
                                </Typography>
                                <Typography variant="h6" component="div" fontWeight="bold">
                                    {batch.unsexedCount.toLocaleString()}
                                </Typography>
                            </Box>
                        </Box>
                    </Box>
                </CardContent>
            </Card>

            {/* Feeding Table */}
            <Card sx={{ mt: 3 }}>
                <CardContent>
                    <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 2 }}>
                        <Typography variant="h6" component="div" fontWeight="bold">
                            Feeding Table
                        </Typography>
                        <Box sx={{ display: "flex", gap: 1 }}>
                            {batch.feedingTable && (
                                <>
                                    <Button
                                        size="small"
                                        variant="outlined"
                                        startIcon={<EditIcon />}
                                        onClick={() => navigate(`/feeding-tables/${batch.feedingTable!.id}`)}
                                    >
                                        Configure
                                    </Button>
                                    <Button
                                        size="small"
                                        variant="outlined"
                                        color="error"
                                        startIcon={<UnlinkIcon />}
                                        onClick={handleUnassignFeedingTable}
                                        disabled={assigning}
                                    >
                                        Unassign
                                    </Button>
                                </>
                            )}
                            <Button
                                size="small"
                                variant="outlined"
                                startIcon={<FeedingTableIcon />}
                                onClick={handleOpenAssignDialog}
                            >
                                {batch.feedingTable ? "Change Table" : "Assign Table"}
                            </Button>
                        </Box>
                    </Box>
                    <Divider sx={{ mb: 2 }} />

                    {/* Daily Feeding Times */}
                    <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", mb: 2 }}>
                        <Box>
                            <Typography variant="body2" color="text.secondary">
                                Daily Feeding Times
                            </Typography>
                            <Typography variant="body1" fontWeight="medium">
                                {batch.dailyFeedingTimes !== null
                                    ? `${batch.dailyFeedingTimes}x / day`
                                    : "Not configured"}
                            </Typography>
                        </Box>
                        <Button
                            size="small"
                            variant="outlined"
                            startIcon={<EditIcon />}
                            onClick={handleOpenEditFeedingTimes}
                        >
                            Edit
                        </Button>
                    </Box>

                    {!batch.feedingTable ? (
                        <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 2 }}>
                            No feeding table assigned to this batch.
                        </Typography>
                    ) : (
                        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                <FeedingTableIcon color="action" />
                                <Box>
                                    <Typography variant="body2" color="text.secondary">
                                        Feeding Table
                                    </Typography>
                                    <Typography
                                        variant="body1"
                                        fontWeight="medium"
                                        sx={{
                                            cursor: "pointer",
                                            color: "primary.main",
                                            textDecoration: "underline",
                                        }}
                                        onClick={() => navigate(`/feeding-tables/${batch.feedingTable!.id}`)}
                                    >
                                        {batch.feedingTable.name}
                                    </Typography>
                                </Box>
                            </Box>

                            {batch.feedingTable && (
                                <Paper
                                    variant="outlined"
                                    sx={{
                                        p: 2,
                                        bgcolor: (theme) =>
                                            currentDayEntry
                                                ? alpha(theme.palette.success.main, 0.08)
                                                : theme.palette.grey[50],
                                        borderColor: currentDayEntry ? "success.light" : "grey.300",
                                    }}
                                >
                                    <Typography variant="subtitle2" fontWeight="bold" gutterBottom>
                                        Today&apos;s Feeding — Day {days}
                                    </Typography>
                                    {currentDayEntry ? (
                                        <>
                                            <Box
                                                sx={{
                                                    display: "grid",
                                                    gridTemplateColumns:
                                                        currentDayEntry.expectedBirdWeight !== null
                                                            ? "1fr 1fr 1fr"
                                                            : "1fr 1fr",
                                                    gap: 2,
                                                    mt: 1,
                                                }}
                                            >
                                                <Box
                                                    sx={{
                                                        p: 1.5,
                                                        borderRadius: 1,
                                                        bgcolor: (theme) => alpha(theme.palette.success.main, 0.16),
                                                        border: "1px solid",
                                                        borderColor: "success.light",
                                                    }}
                                                >
                                                    <Typography
                                                        variant="caption"
                                                        color="text.secondary"
                                                        display="block"
                                                    >
                                                        Food Type
                                                    </Typography>
                                                    <Typography variant="h6" fontWeight="bold" color="success.dark">
                                                        {currentDayEntry.foodType}
                                                    </Typography>
                                                </Box>
                                                <Box
                                                    sx={{
                                                        p: 1.5,
                                                        borderRadius: 1,
                                                        bgcolor: (theme) => alpha(theme.palette.success.main, 0.16),
                                                        border: "1px solid",
                                                        borderColor: "success.light",
                                                    }}
                                                >
                                                    <Typography
                                                        variant="caption"
                                                        color="text.secondary"
                                                        display="block"
                                                    >
                                                        Amount / Bird
                                                    </Typography>
                                                    <Typography variant="h6" fontWeight="bold" color="success.dark">
                                                        {currentDayEntry.amountPerBird}{" "}
                                                        <Typography
                                                            component="span"
                                                            variant="body2"
                                                            color="text.secondary"
                                                        >
                                                            {currentDayEntry.unitOfMeasure}
                                                        </Typography>
                                                    </Typography>
                                                </Box>
                                                {currentDayEntry.expectedBirdWeight !== null && (
                                                    <Box
                                                        sx={{
                                                            p: 1.5,
                                                            borderRadius: 1,
                                                            bgcolor: "grey.100",
                                                            border: "1px solid",
                                                            borderColor: "grey.300",
                                                        }}
                                                    >
                                                        <Typography
                                                            variant="caption"
                                                            color="text.secondary"
                                                            display="block"
                                                        >
                                                            Exp. Bird Weight
                                                        </Typography>
                                                        <Typography variant="h6" fontWeight="bold">
                                                            {currentDayEntry.expectedBirdWeight}{" "}
                                                            <Typography
                                                                component="span"
                                                                variant="body2"
                                                                color="text.secondary"
                                                            >
                                                                {currentDayEntry.expectedBirdWeightUnitOfMeasure}
                                                            </Typography>
                                                        </Typography>
                                                    </Box>
                                                )}
                                            </Box>
                                            {totalBatchAmountPerSession !== null && (
                                                <Box
                                                    sx={{
                                                        mt: 1.5,
                                                        pt: 1.5,
                                                        borderTop: "1px dashed",
                                                        borderColor: "success.light",
                                                    }}
                                                >
                                                    <Typography variant="body2" color="text.secondary">
                                                        Total per Feeding Session
                                                    </Typography>
                                                    <Typography variant="h6" fontWeight="bold" color="success.main">
                                                        {formatTotal(
                                                            totalBatchAmountPerSession,
                                                            currentDayEntry.unitOfMeasure,
                                                        )}{" "}
                                                        {currentDayEntry.unitOfMeasure}
                                                    </Typography>
                                                </Box>
                                            )}
                                            {currentDayEntry.dayNumber < days && (
                                                <Typography
                                                    variant="caption"
                                                    color="text.secondary"
                                                    sx={{ mt: 1, display: "block" }}
                                                >
                                                    Showing last configured day (Day {currentDayEntry.dayNumber}) —
                                                    current day exceeds feeding table.
                                                </Typography>
                                            )}
                                        </>
                                    ) : (
                                        <Typography variant="body2" color="text.secondary">
                                            No feeding entries configured in this table.
                                        </Typography>
                                    )}
                                </Paper>
                            )}

                            {/* Validation: daily feeding times not set */}
                            {!batch.dailyFeedingTimes || batch.dailyFeedingTimes <= 0 ? (
                                <Alert severity="warning" sx={{ mt: 1 }}>
                                    Daily feeding times not configured — calculations assume 1 feeding per day.
                                </Alert>
                            ) : null}

                            {/* View lifecycle plan button */}
                            {sortedFeedingEntries.length > 0 && (
                                <Button
                                    size="small"
                                    variant="outlined"
                                    startIcon={<FeedingTableIcon />}
                                    onClick={() => setLifecyclePlanOpen(true)}
                                >
                                    View Full Lifecycle Feeding Plan
                                </Button>
                            )}
                        </Box>
                    )}
                </CardContent>
            </Card>

            {/* Activities Section */}
            <Card sx={{ mt: 3 }}>
                <CardContent>
                    <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 2 }}>
                        <Typography variant="h6" component="div" fontWeight="bold">
                            Activities
                        </Typography>
                        <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenActivityMenu}>
                            Register Activity
                        </Button>
                    </Box>

                    <Divider sx={{ mb: 2 }} />

                    {activities.length === 0 ? (
                        <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 4 }}>
                            No activities recorded yet.
                        </Typography>
                    ) : (
                        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                            {activities.map((activity) => (
                                <Card key={activity.id} variant="outlined">
                                    <CardContent>
                                        <Box
                                            sx={{
                                                display: "flex",
                                                justifyContent: "space-between",
                                                alignItems: "flex-start",
                                                mb: 1,
                                            }}
                                        >
                                            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                                {activity.type === "MortalityRecording" ? (
                                                    <MortalityActivityIcon color="error" />
                                                ) : activity.type === "StatusSwitch" ? (
                                                    <StatusSwitchIcon color="action" />
                                                ) : activity.type === "ProductConsumption" ? (
                                                    <ProductConsumptionIcon color="primary" />
                                                ) : activity.type === "WeightMeasurement" ? (
                                                    <WeightMeasurementIcon color="success" />
                                                ) : (
                                                    <StatusIcon color="action" />
                                                )}
                                                <Typography variant="subtitle1" fontWeight="medium">
                                                    {activity.type === "MortalityRecording"
                                                        ? "Mortality Registration"
                                                        : activity.type === "StatusSwitch"
                                                          ? "Status Switch"
                                                          : activity.type === "ProductConsumption"
                                                            ? "Product Consumption"
                                                            : activity.type === "WeightMeasurement"
                                                              ? "Weight Measurement"
                                                              : activity.type}
                                                </Typography>
                                            </Box>
                                            <Typography variant="body2" color="text.secondary">
                                                {moment(activity.date).format("MMM DD, YYYY")}
                                            </Typography>
                                        </Box>

                                        {activity.type === "MortalityRecording" && (
                                            <Box sx={{ display: "flex", gap: 2, mt: 1 }}>
                                                <Typography variant="body2">
                                                    <strong>Deaths:</strong>{" "}
                                                    {(activity as MortalityRegistration).numberOfDeaths}
                                                </Typography>
                                                <Typography variant="body2">
                                                    <strong>Sex:</strong> {(activity as MortalityRegistration).sex}
                                                </Typography>
                                            </Box>
                                        )}

                                        {activity.type === "StatusSwitch" && (
                                            <Box sx={{ display: "flex", gap: 2, mt: 1 }}>
                                                <Typography variant="body2">
                                                    <strong>New Status:</strong> {(activity as StatusSwitch).newStatus}
                                                </Typography>
                                            </Box>
                                        )}

                                        {activity.type === "ProductConsumption" && (
                                            <Box sx={{ display: "flex", gap: 2, mt: 1 }}>
                                                <Typography variant="body2">
                                                    <strong>Product:</strong>{" "}
                                                    {(activity as ProductConsumption).productName}
                                                </Typography>
                                                <Typography variant="body2">
                                                    <strong>Stock:</strong> {(activity as ProductConsumption).stock}{" "}
                                                    {(activity as ProductConsumption).unitOfMeasure}
                                                </Typography>
                                            </Box>
                                        )}

                                        {activity.type === "WeightMeasurement" && (
                                            <Box sx={{ display: "flex", gap: 2, mt: 1 }}>
                                                <Typography variant="body2">
                                                    <strong>Average Weight:</strong>{" "}
                                                    {(activity as WeightMeasurement).averageWeight}{" "}
                                                    {(activity as WeightMeasurement).unitOfMeasure}
                                                </Typography>
                                                <Typography variant="body2">
                                                    <strong>Sample Size:</strong>{" "}
                                                    {(activity as WeightMeasurement).sampleSize}
                                                </Typography>
                                            </Box>
                                        )}

                                        {activity.notes && (
                                            <Typography
                                                variant="body2"
                                                color="text.secondary"
                                                sx={{ mt: 1, fontStyle: "italic" }}
                                            >
                                                {activity.notes}
                                            </Typography>
                                        )}
                                    </CardContent>
                                </Card>
                            ))}
                        </Box>
                    )}
                </CardContent>
            </Card>

            {/* Activity Menu */}
            <Menu
                anchorEl={anchorEl}
                open={menuOpen}
                onClose={handleCloseActivityMenu}
                anchorOrigin={{
                    vertical: "bottom",
                    horizontal: "right",
                }}
                transformOrigin={{
                    vertical: "top",
                    horizontal: "right",
                }}
            >
                <MenuItem onClick={() => handleSelectActivity("MortalityRecording")} disabled={!canRegisterMortality()}>
                    <ListItemIcon>
                        <MortalityActivityIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Register Mortality</ListItemText>
                </MenuItem>
                <MenuItem onClick={() => handleSelectActivity("StatusSwitch")} disabled={!canSwitchStatus()}>
                    <ListItemIcon>
                        <StatusSwitchIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Switch Status</ListItemText>
                </MenuItem>
                <MenuItem onClick={() => handleSelectActivity("ProductConsumption")}>
                    <ListItemIcon>
                        <ProductConsumptionIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Register Product Consumption</ListItemText>
                </MenuItem>
                <MenuItem
                    onClick={() => handleSelectActivity("WeightMeasurement")}
                    disabled={!canRegisterWeightMeasurement()}
                >
                    <ListItemIcon>
                        <WeightMeasurementIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Register Weight Measurement</ListItemText>
                </MenuItem>
            </Menu>

            {/* Activity Dialog */}
            {selectedActivityType && (
                <RegisterActivityDialog
                    open={activityDialogOpen}
                    onClose={handleCloseActivityDialog}
                    batch={batch}
                    activityType={selectedActivityType}
                    onSuccess={handleActivitySuccess}
                />
            )}

            {/* Edit Batch Name Dialog */}
            <EditBatchNameDialog
                open={editNameDialogOpen}
                onClose={() => setEditNameDialogOpen(false)}
                batch={batch}
                onSuccess={handleActivitySuccess}
            />

            {/* Edit Batch Notes Dialog */}
            <EditBatchNotesDialog
                open={editNotesDialogOpen}
                onClose={() => setEditNotesDialogOpen(false)}
                batchId={batch.id}
                currentNotes={batch.notes ?? null}
                onSuccess={handleActivitySuccess}
            />

            {/* Edit Daily Feeding Times Dialog */}
            <Dialog open={editFeedingTimesOpen} onClose={() => setEditFeedingTimesOpen(false)} maxWidth="xs" fullWidth>
                <DialogTitle>Daily Feeding Times</DialogTitle>
                <DialogContent>
                    <Box sx={{ pt: 1, display: "flex", flexDirection: "column", gap: 2 }}>
                        {editFeedingTimesError && <Alert severity="error">{editFeedingTimesError}</Alert>}
                        <TextField
                            label="Times per day"
                            type="number"
                            value={editFeedingTimesValue}
                            onChange={(e) => setEditFeedingTimesValue(e.target.value)}
                            placeholder="Leave empty to clear"
                            inputProps={{ min: 1, step: 1 }}
                            helperText="How many times per day this batch is fed"
                        />
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setEditFeedingTimesOpen(false)}>Cancel</Button>
                    <Button variant="contained" onClick={handleSaveFeedingTimes} disabled={savingFeedingTimes}>
                        {savingFeedingTimes ? "Saving..." : "Save"}
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Assign Feeding Table Dialog */}
            <Dialog open={assignDialogOpen} onClose={() => setAssignDialogOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>Assign Feeding Table</DialogTitle>
                <DialogContent>
                    <Box sx={{ pt: 1, display: "flex", flexDirection: "column", gap: 2 }}>
                        {assignError && <Alert severity="error">{assignError}</Alert>}
                        <FormControl fullWidth>
                            <InputLabel>Feeding Table</InputLabel>
                            <Select
                                value={selectedFeedingTableId}
                                label="Feeding Table"
                                onChange={(e) => setSelectedFeedingTableId(e.target.value)}
                            >
                                {availableFeedingTables.length === 0 && (
                                    <MenuItem disabled value="">
                                        No feeding tables available
                                    </MenuItem>
                                )}
                                {availableFeedingTables.map((ft) => (
                                    <MenuItem key={ft.id} value={ft.id}>
                                        {ft.name}
                                        {ft.description ? ` — ${ft.description}` : ""}
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                        <Button
                            size="small"
                            variant="text"
                            startIcon={<AddIcon />}
                            onClick={() => navigate("/feeding-tables")}
                            sx={{ alignSelf: "flex-start" }}
                        >
                            Create a new feeding table
                        </Button>
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setAssignDialogOpen(false)}>Cancel</Button>
                    <Button
                        variant="contained"
                        onClick={handleConfirmAssign}
                        disabled={assigning || !selectedFeedingTableId}
                    >
                        {assigning ? "Assigning..." : "Assign"}
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Lifecycle Feeding Plan Dialog */}
            <Dialog open={lifecyclePlanOpen} onClose={() => setLifecyclePlanOpen(false)} maxWidth="md" fullWidth>
                <DialogTitle>
                    Full Lifecycle Feeding Plan
                    <Typography variant="body2" color="text.secondary">
                        {batch.feedingTable?.name} — {batch.population.toLocaleString()} birds (current population)
                    </Typography>
                </DialogTitle>
                <DialogContent sx={{ px: 2, pb: 1 }}>
                    {(!batch.dailyFeedingTimes || batch.dailyFeedingTimes <= 0) && (
                        <Alert severity="warning" sx={{ mb: 2 }}>
                            Daily feeding times not configured — calculations assume 1 feeding per day.
                        </Alert>
                    )}
                    <TableContainer sx={{ maxHeight: 500 }}>
                        <Table size="small" stickyHeader>
                            <TableHead>
                                <TableRow>
                                    <TableCell sx={{ fontWeight: "bold" }}>Day</TableCell>
                                    <TableCell sx={{ fontWeight: "bold" }}>Food Type</TableCell>
                                    <TableCell sx={{ fontWeight: "bold" }}>Amount / Bird</TableCell>
                                    <TableCell sx={{ fontWeight: "bold" }}>Total / Day</TableCell>
                                    <TableCell sx={{ fontWeight: "bold", textAlign: "center" }}>
                                        <Tooltip title="Daily feeding times configured for this batch">
                                            <span>Times / Day</span>
                                        </Tooltip>
                                    </TableCell>
                                    <TableCell sx={{ fontWeight: "bold" }}>Per Session</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {sortedFeedingEntries.map((entry) => {
                                    const isToday =
                                        entry.dayNumber === days ||
                                        (entry.dayNumber === currentDayEntry?.dayNumber &&
                                            currentDayEntry?.dayNumber !== undefined);
                                    const totalPerDay = calcEntryTotalPerDay(entry);
                                    const perSession = calcEntryPerSession(entry);
                                    return (
                                        <TableRow
                                            key={entry.id}
                                            sx={
                                                isToday
                                                    ? {
                                                          bgcolor: (theme) => alpha(theme.palette.success.main, 0.08),
                                                          "& td": {
                                                              borderColor: "success.light",
                                                              fontWeight: "bold",
                                                          },
                                                      }
                                                    : undefined
                                            }
                                        >
                                            <TableCell>
                                                {isToday ? (
                                                    <Tooltip title="Today">
                                                        <Box
                                                            component="span"
                                                            sx={{ display: "flex", alignItems: "center", gap: 0.5 }}
                                                        >
                                                            {entry.dayNumber}
                                                            <Box
                                                                component="span"
                                                                sx={{
                                                                    fontSize: "0.7rem",
                                                                    color: "success.main",
                                                                    fontWeight: "bold",
                                                                }}
                                                            >
                                                                ★
                                                            </Box>
                                                        </Box>
                                                    </Tooltip>
                                                ) : (
                                                    entry.dayNumber
                                                )}
                                            </TableCell>
                                            <TableCell>{entry.foodType}</TableCell>
                                            <TableCell>
                                                {entry.amountPerBird} {entry.unitOfMeasure}
                                            </TableCell>
                                            <TableCell>
                                                {formatTotal(totalPerDay, entry.unitOfMeasure)} {entry.unitOfMeasure}
                                            </TableCell>
                                            <TableCell sx={{ textAlign: "center" }}>
                                                {effectiveDailyFeedingTimes}
                                            </TableCell>
                                            <TableCell>
                                                {formatTotal(perSession, entry.unitOfMeasure)} {entry.unitOfMeasure}
                                            </TableCell>
                                        </TableRow>
                                    );
                                })}
                            </TableBody>
                        </Table>
                    </TableContainer>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setLifecyclePlanOpen(false)}>Close</Button>
                </DialogActions>
            </Dialog>
        </Container>
    );
}
