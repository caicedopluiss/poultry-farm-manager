import { useState } from "react";
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
    Restaurant as FeedingIcon,
} from "@mui/icons-material";
import moment from "moment";
import type { Batch } from "../../types/batch";
import type { BatchActivityType } from "../../types/batchActivity";
import RegisterActivityDialog from "../RegisterActivityDialog";

interface BatchDetailProps {
    batch: Batch;
    onRefresh?: () => void;
}

export default function BatchDetail({ batch, onRefresh }: BatchDetailProps) {
    const navigate = useNavigate();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    const [activityDialogOpen, setActivityDialogOpen] = useState(false);
    const [selectedActivityType, setSelectedActivityType] = useState<BatchActivityType | null>(null);
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const menuOpen = Boolean(anchorEl);

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

    const days = calculateDays(batch.startDate);
    const weeks = calculateWeeks(batch.startDate);
    const mortalityPercent = calculateMortality(batch.initialPopulation, batch.population);

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

    return (
        <Container maxWidth="lg" sx={{ py: 3 }}>
            {/* Header */}
            <Box sx={{ mb: 4 }}>
                <Button variant="outlined" startIcon={<BackIcon />} onClick={() => navigate("/")} sx={{ mb: 2 }}>
                    Back to Batches
                </Button>

                <Box sx={{ display: "flex", alignItems: "center", gap: 2, mb: 2 }}>
                    <Typography
                        variant={isMobile ? "h4" : "h3"}
                        component="h1"
                        fontWeight="bold"
                        sx={{ color: theme.palette.primary.main }}
                    >
                        {batch.name}
                    </Typography>
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
                        <Typography variant="h6" gutterBottom fontWeight="bold">
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
                        </Box>
                    </CardContent>
                </Card>

                {/* Time Metrics */}
                <Card>
                    <CardContent>
                        <Typography variant="h6" gutterBottom fontWeight="bold">
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
                                        Weeks
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
                    <Typography variant="h6" gutterBottom fontWeight="bold">
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
                                bgcolor: mortalityPercent > 10 ? "error.50" : "success.50",
                                border: "1px solid",
                                borderColor: mortalityPercent > 10 ? "error.200" : "success.200",
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
                                <Typography variant="h6" fontWeight="bold" color="info.main">
                                    {batch.maleCount.toLocaleString()}
                                </Typography>
                            </Box>
                            <Box sx={{ textAlign: "center" }}>
                                <Typography variant="body2" color="text.secondary">
                                    Female
                                </Typography>
                                <Typography variant="h6" fontWeight="bold" color="secondary.main">
                                    {batch.femaleCount.toLocaleString()}
                                </Typography>
                            </Box>
                            <Box sx={{ textAlign: "center" }}>
                                <Typography variant="body2" color="text.secondary">
                                    Unsexed
                                </Typography>
                                <Typography variant="h6" fontWeight="bold">
                                    {batch.unsexedCount.toLocaleString()}
                                </Typography>
                            </Box>
                        </Box>
                    </Box>
                </CardContent>
            </Card>

            {/* Activities Section */}
            <Card sx={{ mt: 3 }}>
                <CardContent>
                    <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 2 }}>
                        <Typography variant="h6" fontWeight="bold">
                            Activities
                        </Typography>
                        <Button
                            variant="contained"
                            startIcon={<AddIcon />}
                            onClick={handleOpenActivityMenu}
                            disabled={batch.status.toLowerCase() !== "active"}
                        >
                            Register Activity
                        </Button>
                    </Box>

                    <Divider sx={{ mb: 2 }} />

                    <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 4 }}>
                        Activity history will be displayed here.
                        {batch.status.toLowerCase() !== "active" && (
                            <Box component="span" sx={{ display: "block", mt: 1 }}>
                                (Activity registration is only available for active batches)
                            </Box>
                        )}
                    </Typography>
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
                <MenuItem onClick={() => handleSelectActivity("MortalityRecording")}>
                    <ListItemIcon>
                        <MortalityActivityIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Register Mortality</ListItemText>
                </MenuItem>
                <MenuItem onClick={() => handleSelectActivity("Feeding")} disabled>
                    <ListItemIcon>
                        <FeedingIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Register Feeding (Coming Soon)</ListItemText>
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
        </Container>
    );
}
