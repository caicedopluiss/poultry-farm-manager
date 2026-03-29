import { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import {
    Container,
    Typography,
    Box,
    Button,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    IconButton,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    Select,
    MenuItem,
    FormControl,
    Alert,
    CircularProgress,
    Tooltip,
} from "@mui/material";
import {
    Add as AddIcon,
    Refresh as RefreshIcon,
    Edit as EditIcon,
    Delete as DeleteRowIcon,
    SetMeal as FeedingTableIcon,
} from "@mui/icons-material";
import { getFeedingTables, createFeedingTable } from "@/api/v1/feedingTables";
import type { FeedingTable, NewFeedingTableDayEntry } from "@/types/feedingTable";

const FOOD_TYPE_OPTIONS = ["PreInicio", "Inicio", "Engorde"];
const UOM_OPTIONS = ["Kilogram", "Gram", "Pound"];

interface DayEntryForm {
    foodType: string;
    amountPerBird: string;
    unitOfMeasure: string;
    expectedBirdWeight: string;
    expectedBirdWeightUnitOfMeasure: string;
}

const emptyEntry = (): DayEntryForm => ({
    foodType: "Inicio",
    amountPerBird: "",
    unitOfMeasure: "Kilogram",
    expectedBirdWeight: "",
    expectedBirdWeightUnitOfMeasure: "Kilogram",
});

export default function FeedingTableListPage() {
    const navigate = useNavigate();
    const [feedingTables, setFeedingTables] = useState<FeedingTable[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [createOpen, setCreateOpen] = useState(false);

    // Create form state
    const [newName, setNewName] = useState("");
    const [newDescription, setNewDescription] = useState("");
    const [dayEntries, setDayEntries] = useState<DayEntryForm[]>([emptyEntry()]);
    const [creating, setCreating] = useState(false);
    const [createError, setCreateError] = useState<string | null>(null);

    const loadFeedingTables = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const { feedingTables: data } = await getFeedingTables();
            setFeedingTables(data);
        } catch {
            setError("Failed to load feeding tables.");
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        loadFeedingTables();
    }, [loadFeedingTables]);

    const handleOpenCreate = () => {
        setNewName("");
        setNewDescription("");
        setDayEntries([emptyEntry()]);
        setCreateError(null);
        setCreateOpen(true);
    };

    const handleAddEntry = () => {
        setDayEntries((prev) => [...prev, emptyEntry()]);
    };

    const handleRemoveEntry = (index: number) => {
        setDayEntries((prev) => prev.filter((_, i) => i !== index));
    };

    const handleEntryChange = (index: number, field: keyof DayEntryForm, value: string) => {
        setDayEntries((prev) => prev.map((entry, i) => (i === index ? { ...entry, [field]: value } : entry)));
    };

    const handleCreate = async () => {
        setCreateError(null);
        if (!newName.trim()) {
            setCreateError("Name is required.");
            return;
        }
        if (dayEntries.length === 0) {
            setCreateError("At least one day entry is required.");
            return;
        }
        const parsedEntries: NewFeedingTableDayEntry[] = [];
        for (const [index, entry] of dayEntries.entries()) {
            const dayNum = index + 1;
            const amt = parseFloat(entry.amountPerBird);
            if (isNaN(amt) || amt <= 0) {
                setCreateError("Amount must be a positive number.");
                return;
            }
            const weight = entry.expectedBirdWeight ? parseFloat(entry.expectedBirdWeight) : null;
            if (weight !== null && (isNaN(weight) || weight <= 0)) {
                setCreateError("Expected bird weight must be a positive number.");
                return;
            }
            parsedEntries.push({
                dayNumber: dayNum,
                foodType: entry.foodType,
                amountPerBird: amt,
                unitOfMeasure: entry.unitOfMeasure,
                expectedBirdWeight: weight,
                expectedBirdWeightUnitOfMeasure: weight !== null ? entry.expectedBirdWeightUnitOfMeasure : null,
            });
        }
        setCreating(true);
        try {
            await createFeedingTable({
                name: newName.trim(),
                description: newDescription.trim() || null,
                dayEntries: parsedEntries,
            });
            setCreateOpen(false);
            loadFeedingTables();
        } catch (err: unknown) {
            const apiErr = err as { response?: { message?: string } };
            setCreateError(apiErr?.response?.message || "Failed to create feeding table.");
        } finally {
            setCreating(false);
        }
    };

    return (
        <Container maxWidth="lg" sx={{ py: 3 }}>
            <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
                <Box>
                    <Typography variant="h4" component="h1" fontWeight="bold">
                        Feeding Tables
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                        Manage feeding schedules to assign to batches.
                    </Typography>
                </Box>
                <Box sx={{ display: "flex", gap: 1 }}>
                    <Button
                        variant="outlined"
                        startIcon={<RefreshIcon />}
                        onClick={loadFeedingTables}
                        disabled={loading}
                    >
                        Refresh
                    </Button>
                    <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
                        Create Feeding Table
                    </Button>
                </Box>
            </Box>

            {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                    {error}
                </Alert>
            )}

            {loading ? (
                <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
                    <CircularProgress />
                </Box>
            ) : feedingTables.length === 0 ? (
                <Paper
                    sx={{
                        p: 6,
                        textAlign: "center",
                        border: "2px dashed",
                        borderColor: "divider",
                    }}
                >
                    <FeedingTableIcon sx={{ fontSize: 64, color: "text.disabled", mb: 2 }} />
                    <Typography variant="h6" color="text.secondary" gutterBottom>
                        No feeding tables yet
                    </Typography>
                    <Typography variant="body2" color="text.disabled" sx={{ mb: 2 }}>
                        Create a feeding table to manage feeding schedules for your batches.
                    </Typography>
                    <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
                        Create Feeding Table
                    </Button>
                </Paper>
            ) : (
                <TableContainer component={Paper}>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell>
                                    <strong>Name</strong>
                                </TableCell>
                                <TableCell>
                                    <strong>Description</strong>
                                </TableCell>
                                <TableCell align="center">
                                    <strong>Day Entries</strong>
                                </TableCell>
                                <TableCell align="center">
                                    <strong>Actions</strong>
                                </TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {feedingTables.map((table) => (
                                <TableRow
                                    key={table.id}
                                    hover
                                    sx={{ cursor: "pointer" }}
                                    onClick={() => navigate(`/feeding-tables/${table.id}`)}
                                >
                                    <TableCell>
                                        <Typography variant="body1" fontWeight="medium">
                                            {table.name}
                                        </Typography>
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="body2" color="text.secondary">
                                            {table.description || "—"}
                                        </Typography>
                                    </TableCell>
                                    <TableCell align="center">
                                        <Typography variant="body2">{table.dayEntries.length}</Typography>
                                    </TableCell>
                                    <TableCell align="center">
                                        <Tooltip title="Edit">
                                            <IconButton
                                                size="small"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    navigate(`/feeding-tables/${table.id}`);
                                                }}
                                            >
                                                <EditIcon />
                                            </IconButton>
                                        </Tooltip>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}

            {/* Create Dialog */}
            <Dialog open={createOpen} onClose={() => setCreateOpen(false)} maxWidth="md" fullWidth>
                <DialogTitle>Create Feeding Table</DialogTitle>
                <DialogContent>
                    <Box sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
                        {createError && <Alert severity="error">{createError}</Alert>}
                        <TextField
                            label="Name"
                            value={newName}
                            onChange={(e) => setNewName(e.target.value)}
                            required
                            inputProps={{ maxLength: 100 }}
                        />
                        <TextField
                            label="Description"
                            value={newDescription}
                            onChange={(e) => setNewDescription(e.target.value)}
                            multiline
                            rows={2}
                            inputProps={{ maxLength: 500 }}
                        />
                        <Box>
                            <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 1 }}>
                                <Typography variant="subtitle1" fontWeight="bold">
                                    Day Entries
                                </Typography>
                                <Button size="small" startIcon={<AddIcon />} onClick={handleAddEntry}>
                                    Add Entry
                                </Button>
                            </Box>
                            <TableContainer component={Paper} variant="outlined">
                                <Table size="small">
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>Day #</TableCell>
                                            <TableCell>Food Type</TableCell>
                                            <TableCell>Amount / Bird</TableCell>
                                            <TableCell>Unit</TableCell>
                                            <TableCell>Exp. Weight</TableCell>
                                            <TableCell>Weight Unit</TableCell>
                                            <TableCell />
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {dayEntries.map((entry, index) => (
                                            <TableRow key={index}>
                                                <TableCell>
                                                    <Typography variant="body2" fontWeight="medium" sx={{ px: 1 }}>
                                                        {index + 1}
                                                    </Typography>
                                                </TableCell>
                                                <TableCell>
                                                    <FormControl size="small" sx={{ minWidth: 120 }}>
                                                        <Select
                                                            value={entry.foodType}
                                                            onChange={(e) =>
                                                                handleEntryChange(index, "foodType", e.target.value)
                                                            }
                                                        >
                                                            {FOOD_TYPE_OPTIONS.map((ft) => (
                                                                <MenuItem key={ft} value={ft}>
                                                                    {ft}
                                                                </MenuItem>
                                                            ))}
                                                        </Select>
                                                    </FormControl>
                                                </TableCell>
                                                <TableCell>
                                                    <TextField
                                                        size="small"
                                                        type="number"
                                                        value={entry.amountPerBird}
                                                        onChange={(e) =>
                                                            handleEntryChange(index, "amountPerBird", e.target.value)
                                                        }
                                                        inputProps={{ min: 0.001, step: 0.001 }}
                                                        sx={{ width: 90 }}
                                                    />
                                                </TableCell>
                                                <TableCell>
                                                    <FormControl size="small" sx={{ minWidth: 110 }}>
                                                        <Select
                                                            value={entry.unitOfMeasure}
                                                            onChange={(e) =>
                                                                handleEntryChange(
                                                                    index,
                                                                    "unitOfMeasure",
                                                                    e.target.value,
                                                                )
                                                            }
                                                        >
                                                            {UOM_OPTIONS.map((uom) => (
                                                                <MenuItem key={uom} value={uom}>
                                                                    {uom}
                                                                </MenuItem>
                                                            ))}
                                                        </Select>
                                                    </FormControl>
                                                </TableCell>
                                                <TableCell>
                                                    <TextField
                                                        size="small"
                                                        type="number"
                                                        placeholder="Optional"
                                                        value={entry.expectedBirdWeight}
                                                        onChange={(e) =>
                                                            handleEntryChange(
                                                                index,
                                                                "expectedBirdWeight",
                                                                e.target.value,
                                                            )
                                                        }
                                                        inputProps={{ min: 0.001, step: 0.001 }}
                                                        sx={{ width: 100 }}
                                                    />
                                                </TableCell>
                                                <TableCell>
                                                    <FormControl
                                                        size="small"
                                                        sx={{ minWidth: 110 }}
                                                        disabled={!entry.expectedBirdWeight}
                                                    >
                                                        <Select
                                                            value={entry.expectedBirdWeightUnitOfMeasure}
                                                            onChange={(e) =>
                                                                handleEntryChange(
                                                                    index,
                                                                    "expectedBirdWeightUnitOfMeasure",
                                                                    e.target.value,
                                                                )
                                                            }
                                                        >
                                                            {UOM_OPTIONS.map((uom) => (
                                                                <MenuItem key={uom} value={uom}>
                                                                    {uom}
                                                                </MenuItem>
                                                            ))}
                                                        </Select>
                                                    </FormControl>
                                                </TableCell>
                                                <TableCell>
                                                    <IconButton
                                                        size="small"
                                                        color="error"
                                                        onClick={() => handleRemoveEntry(index)}
                                                        disabled={dayEntries.length === 1}
                                                    >
                                                        <DeleteRowIcon />
                                                    </IconButton>
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        </Box>
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setCreateOpen(false)}>Cancel</Button>
                    <Button variant="contained" onClick={handleCreate} disabled={creating}>
                        {creating ? "Creating..." : "Create"}
                    </Button>
                </DialogActions>
            </Dialog>
        </Container>
    );
}
