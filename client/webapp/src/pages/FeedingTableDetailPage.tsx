import { useState, useEffect, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
    Container,
    Typography,
    Box,
    Button,
    Card,
    CardContent,
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
    Chip,
    Divider,
} from "@mui/material";
import { ArrowBack as BackIcon, Edit as EditIcon, Add as AddIcon, Delete as DeleteRowIcon } from "@mui/icons-material";
import { getFeedingTableById, updateFeedingTable } from "@/api/v1/feedingTables";
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

const toDayEntryForm = (entry: {
    dayNumber: number;
    foodType: string;
    amountPerBird: number;
    unitOfMeasure: string;
    expectedBirdWeight?: number | null;
    expectedBirdWeightUnitOfMeasure?: string | null;
}): DayEntryForm => ({
    foodType: entry.foodType,
    amountPerBird: String(entry.amountPerBird),
    unitOfMeasure: entry.unitOfMeasure,
    expectedBirdWeight:
        entry.expectedBirdWeight !== null && entry.expectedBirdWeight !== undefined
            ? String(entry.expectedBirdWeight)
            : "",
    expectedBirdWeightUnitOfMeasure: entry.expectedBirdWeightUnitOfMeasure ?? "Kilogram",
});

export default function FeedingTableDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [feedingTable, setFeedingTable] = useState<FeedingTable | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Edit info dialog
    const [editInfoOpen, setEditInfoOpen] = useState(false);
    const [editName, setEditName] = useState("");
    const [editDescription, setEditDescription] = useState("");
    const [savingInfo, setSavingInfo] = useState(false);
    const [editInfoError, setEditInfoError] = useState<string | null>(null);

    // Edit day entries dialog
    const [editEntriesOpen, setEditEntriesOpen] = useState(false);
    const [editDayEntries, setEditDayEntries] = useState<DayEntryForm[]>([]);
    const [savingEntries, setSavingEntries] = useState(false);
    const [editEntriesError, setEditEntriesError] = useState<string | null>(null);

    const loadFeedingTable = useCallback(async () => {
        if (!id) return;
        setLoading(true);
        setError(null);
        try {
            const { feedingTable: data } = await getFeedingTableById(id);
            if (!data) {
                setError("Feeding table not found.");
            } else {
                setFeedingTable(data);
            }
        } catch {
            setError("Failed to load feeding table.");
        } finally {
            setLoading(false);
        }
    }, [id]);

    useEffect(() => {
        loadFeedingTable();
    }, [loadFeedingTable]);

    const handleOpenEditInfo = () => {
        if (!feedingTable) return;
        setEditName(feedingTable.name);
        setEditDescription(feedingTable.description || "");
        setEditInfoError(null);
        setEditInfoOpen(true);
    };

    const handleSaveInfo = async () => {
        if (!feedingTable) return;
        if (!editName.trim()) {
            setEditInfoError("Name is required.");
            return;
        }
        setSavingInfo(true);
        setEditInfoError(null);
        try {
            const { feedingTable: updated } = await updateFeedingTable(feedingTable.id, {
                name: editName.trim(),
                description: editDescription.trim() || null,
            });
            setFeedingTable(updated);
            setEditInfoOpen(false);
        } catch (err: unknown) {
            const apiErr = err as { response?: { message?: string } };
            setEditInfoError(apiErr?.response?.message || "Failed to update feeding table.");
        } finally {
            setSavingInfo(false);
        }
    };

    const handleOpenEditEntries = () => {
        if (!feedingTable) return;
        const sortedEntries = [...feedingTable.dayEntries].sort((a, b) => a.dayNumber - b.dayNumber);
        setEditDayEntries(sortedEntries.map(toDayEntryForm));
        setEditEntriesError(null);
        setEditEntriesOpen(true);
    };

    const handleAddEntryRow = () => {
        setEditDayEntries((prev) => [...prev, emptyEntry()]);
    };

    const handleRemoveEntryRow = (index: number) => {
        setEditDayEntries((prev) => prev.filter((_, i) => i !== index));
    };

    const handleEntryRowChange = (index: number, field: keyof DayEntryForm, value: string) => {
        setEditDayEntries((prev) => prev.map((e, i) => (i === index ? { ...e, [field]: value } : e)));
    };

    const handleSaveEntries = async () => {
        if (!feedingTable) return;
        if (editDayEntries.length === 0) {
            setEditEntriesError("At least one day entry is required.");
            return;
        }
        const parsedEntries: NewFeedingTableDayEntry[] = [];
        for (const [index, entry] of editDayEntries.entries()) {
            const dayNum = index + 1;
            const amt = parseFloat(entry.amountPerBird);
            if (isNaN(amt) || amt <= 0) {
                setEditEntriesError("All amounts must be positive numbers.");
                return;
            }
            const weight = entry.expectedBirdWeight ? parseFloat(entry.expectedBirdWeight) : null;
            if (weight !== null && (isNaN(weight) || weight <= 0)) {
                setEditEntriesError("Expected bird weight must be a positive number.");
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
        setSavingEntries(true);
        setEditEntriesError(null);
        try {
            const { feedingTable: updated } = await updateFeedingTable(feedingTable.id, {
                dayEntries: parsedEntries,
            });
            setFeedingTable(updated);
            setEditEntriesOpen(false);
        } catch (err: unknown) {
            const apiErr = err as { response?: { message?: string } };
            setEditEntriesError(apiErr?.response?.message || "Failed to update day entries.");
        } finally {
            setSavingEntries(false);
        }
    };

    if (loading) {
        return (
            <Container maxWidth="lg" sx={{ py: 3 }}>
                <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
                    <CircularProgress />
                </Box>
            </Container>
        );
    }

    if (error || !feedingTable) {
        return (
            <Container maxWidth="lg" sx={{ py: 3 }}>
                <Button
                    variant="outlined"
                    startIcon={<BackIcon />}
                    onClick={() => navigate("/feeding-tables")}
                    sx={{ mb: 2 }}
                >
                    Back to Feeding Tables
                </Button>
                <Alert severity="error">{error || "Feeding table not found"}</Alert>
            </Container>
        );
    }

    const sortedEntries = [...feedingTable.dayEntries].sort((a, b) => a.dayNumber - b.dayNumber);

    return (
        <Container maxWidth="lg" sx={{ py: 3 }}>
            {/* Header */}
            <Box sx={{ mb: 3 }}>
                <Button
                    variant="outlined"
                    startIcon={<BackIcon />}
                    onClick={() => navigate("/feeding-tables")}
                    sx={{ mb: 2 }}
                >
                    Back to Feeding Tables
                </Button>
                <Box sx={{ display: "flex", alignItems: "center", gap: 2, flexWrap: "wrap" }}>
                    <Typography variant="h4" component="h1" fontWeight="bold" color="primary">
                        {feedingTable.name}
                    </Typography>
                    <Button size="small" variant="outlined" startIcon={<EditIcon />} onClick={handleOpenEditInfo}>
                        Edit
                    </Button>
                </Box>
                {feedingTable.description && (
                    <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
                        {feedingTable.description}
                    </Typography>
                )}
            </Box>

            {/* Day Entries */}
            <Card>
                <CardContent>
                    <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 1 }}>
                        <Typography variant="h6" fontWeight="bold">
                            Day Entries
                        </Typography>
                        <Box sx={{ display: "flex", gap: 1, alignItems: "center" }}>
                            <Chip label={`${feedingTable.dayEntries.length} entries`} size="small" />
                            <Button
                                size="small"
                                variant="outlined"
                                startIcon={<EditIcon />}
                                onClick={handleOpenEditEntries}
                            >
                                Edit Entries
                            </Button>
                        </Box>
                    </Box>
                    <Divider sx={{ mb: 2 }} />
                    {sortedEntries.length === 0 ? (
                        <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 3 }}>
                            No day entries configured.
                        </Typography>
                    ) : (
                        <TableContainer component={Paper} variant="outlined">
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell>
                                            <strong>Day</strong>
                                        </TableCell>
                                        <TableCell>
                                            <strong>Food Type</strong>
                                        </TableCell>
                                        <TableCell align="right">
                                            <strong>Amount / Bird</strong>
                                        </TableCell>
                                        <TableCell>
                                            <strong>Unit</strong>
                                        </TableCell>
                                        <TableCell>
                                            <strong>Exp. Bird Weight</strong>
                                        </TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {sortedEntries.map((entry) => (
                                        <TableRow key={entry.id}>
                                            <TableCell>
                                                <Chip
                                                    label={`Day ${entry.dayNumber}`}
                                                    size="small"
                                                    variant="outlined"
                                                />
                                            </TableCell>
                                            <TableCell>{entry.foodType}</TableCell>
                                            <TableCell align="right">{entry.amountPerBird}</TableCell>
                                            <TableCell>{entry.unitOfMeasure}</TableCell>
                                            <TableCell>
                                                {entry.expectedBirdWeight !== null &&
                                                entry.expectedBirdWeight !== undefined
                                                    ? `${entry.expectedBirdWeight} ${entry.expectedBirdWeightUnitOfMeasure}`
                                                    : "—"}
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    )}
                </CardContent>
            </Card>

            {/* Edit Info Dialog */}
            <Dialog open={editInfoOpen} onClose={() => setEditInfoOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>Edit Feeding Table</DialogTitle>
                <DialogContent>
                    <Box sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
                        {editInfoError && <Alert severity="error">{editInfoError}</Alert>}
                        <TextField
                            label="Name"
                            value={editName}
                            onChange={(e) => setEditName(e.target.value)}
                            required
                            inputProps={{ maxLength: 100 }}
                        />
                        <TextField
                            label="Description"
                            value={editDescription}
                            onChange={(e) => setEditDescription(e.target.value)}
                            multiline
                            rows={3}
                            inputProps={{ maxLength: 500 }}
                        />
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setEditInfoOpen(false)}>Cancel</Button>
                    <Button variant="contained" onClick={handleSaveInfo} disabled={savingInfo}>
                        {savingInfo ? "Saving..." : "Save"}
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Edit Day Entries Dialog */}
            <Dialog open={editEntriesOpen} onClose={() => setEditEntriesOpen(false)} maxWidth="md" fullWidth>
                <DialogTitle>Edit Day Entries</DialogTitle>
                <DialogContent>
                    <Box sx={{ pt: 1 }}>
                        {editEntriesError && (
                            <Alert severity="error" sx={{ mb: 2 }}>
                                {editEntriesError}
                            </Alert>
                        )}
                        <Box sx={{ display: "flex", justifyContent: "flex-end", mb: 1 }}>
                            <Button size="small" startIcon={<AddIcon />} onClick={handleAddEntryRow}>
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
                                    {editDayEntries.map((entry, index) => (
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
                                                            handleEntryRowChange(index, "foodType", e.target.value)
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
                                                        handleEntryRowChange(index, "amountPerBird", e.target.value)
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
                                                            handleEntryRowChange(index, "unitOfMeasure", e.target.value)
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
                                                        handleEntryRowChange(
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
                                                            handleEntryRowChange(
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
                                                    onClick={() => handleRemoveEntryRow(index)}
                                                    disabled={editDayEntries.length === 1}
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
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setEditEntriesOpen(false)}>Cancel</Button>
                    <Button variant="contained" onClick={handleSaveEntries} disabled={savingEntries}>
                        {savingEntries ? "Saving..." : "Save Entries"}
                    </Button>
                </DialogActions>
            </Dialog>
        </Container>
    );
}
