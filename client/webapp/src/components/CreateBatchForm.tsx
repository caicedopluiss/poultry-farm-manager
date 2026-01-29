import React, { useState } from "react";
import {
    Box,
    Button,
    TextField,
    Typography,
    Alert,
    CircularProgress,
    Stack,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    useTheme,
    useMediaQuery,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
} from "@mui/material";
import type { NewBatch } from "@/types/batch";
import type { Vendor } from "@/types/vendor";
import moment from "moment";

interface CreateBatchFormProps {
    open: boolean;
    onSubmit: (batchData: NewBatch) => void;
    onClose: () => void;
    loading: boolean;
    error: string | null;
    vendors: Vendor[];
    vendorsLoading: boolean;
}

export default function CreateBatchForm({
    open,
    onSubmit,
    onClose,
    loading,
    error,
    vendors,
    vendorsLoading,
}: CreateBatchFormProps) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const isSmallMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const [formData, setFormData] = useState({
        name: "",
        startDate: new Date().toISOString().split("T")[0], // Default to today's date in YYYY-MM-DD format
        maleCount: 0,
        femaleCount: 0,
        unsexedCount: 0,
        breed: "",
        shed: "",
        vendorId: "",
        initialCost: "",
    });

    // Reset form when modal opens
    React.useEffect(() => {
        if (open) {
            setFormData({
                name: "",
                startDate: new Date().toISOString().split("T")[0],
                maleCount: 0,
                femaleCount: 0,
                unsexedCount: 0,
                breed: "",
                shed: "",
                vendorId: "",
                initialCost: "",
            });
        }
    }, [open]);

    const handleInputChange = (field: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.type === "number" ? parseInt(event.target.value) || 0 : event.target.value;
        setFormData((prev) => ({
            ...prev,
            [field]: value,
        }));
    };

    const handleSubmit = (event: React.FormEvent) => {
        event.preventDefault();

        // Convert date string to ISO date
        const date = moment(formData.startDate);

        const newBatch: NewBatch = {
            name: formData.name,
            startClientDateIsoString: date.format(),
            maleCount: formData.maleCount,
            femaleCount: formData.femaleCount,
            unsexedCount: formData.unsexedCount,
            breed: formData.breed || null,
            shed: formData.shed || null,
            vendorId: formData.vendorId,
            initialCost: parseFloat(formData.initialCost),
        };

        onSubmit(newBatch);
    };

    const totalPopulation = formData.maleCount + formData.femaleCount + formData.unsexedCount;

    return (
        <Dialog
            open={open}
            onClose={onClose}
            maxWidth="md"
            fullWidth
            fullScreen={isSmallMobile}
            PaperProps={{
                sx: {
                    minHeight: isMobile ? "90vh" : "auto",
                    margin: isMobile ? 1 : 3,
                },
            }}
        >
            <DialogTitle sx={{ pb: 1 }}>
                <Typography variant={isSmallMobile ? "h5" : "h4"} component="div" sx={{ fontWeight: "bold" }}>
                    Create New Batch
                </Typography>
            </DialogTitle>

            <DialogContent sx={{ px: { xs: 2, sm: 3 }, pb: 1 }}>
                {error && (
                    <Alert severity="error" sx={{ mb: 3 }}>
                        {error}
                    </Alert>
                )}

                <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2 }}>
                    <Stack spacing={4}>
                        <TextField
                            fullWidth
                            label="Batch Name"
                            value={formData.name}
                            onChange={handleInputChange("name")}
                            required
                            disabled={loading}
                            size="medium"
                            sx={{
                                "& .MuiInputBase-root": {
                                    height: isMobile ? 56 : 64,
                                    fontSize: isMobile ? "1rem" : "1.1rem",
                                },
                            }}
                        />

                        <TextField
                            fullWidth
                            label="Start Date"
                            type="date"
                            value={formData.startDate}
                            onChange={handleInputChange("startDate")}
                            required
                            disabled={loading}
                            size="medium"
                            InputLabelProps={{
                                shrink: true,
                            }}
                            sx={{
                                "& .MuiInputBase-root": {
                                    height: isMobile ? 56 : 64,
                                    fontSize: isMobile ? "1rem" : "1.1rem",
                                },
                            }}
                        />

                        <Stack direction={isSmallMobile ? "column" : "row"} spacing={isSmallMobile ? 3 : 2}>
                            <TextField
                                label="Male Count"
                                type="number"
                                value={formData.maleCount}
                                onChange={handleInputChange("maleCount")}
                                inputProps={{ min: 0 }}
                                disabled={loading}
                                size="medium"
                                sx={{
                                    flex: 1,
                                    "& .MuiInputBase-root": {
                                        height: isMobile ? 56 : 64,
                                        fontSize: isMobile ? "1rem" : "1.1rem",
                                    },
                                }}
                            />

                            <TextField
                                label="Female Count"
                                type="number"
                                value={formData.femaleCount}
                                onChange={handleInputChange("femaleCount")}
                                inputProps={{ min: 0 }}
                                disabled={loading}
                                size="medium"
                                sx={{
                                    flex: 1,
                                    "& .MuiInputBase-root": {
                                        height: isMobile ? 56 : 64,
                                        fontSize: isMobile ? "1rem" : "1.1rem",
                                    },
                                }}
                            />

                            <TextField
                                label="Unsexed Count"
                                type="number"
                                value={formData.unsexedCount}
                                onChange={handleInputChange("unsexedCount")}
                                inputProps={{ min: 0 }}
                                disabled={loading}
                                size="medium"
                                sx={{
                                    flex: 1,
                                    "& .MuiInputBase-root": {
                                        height: isMobile ? 56 : 64,
                                        fontSize: isMobile ? "1rem" : "1.1rem",
                                    },
                                }}
                            />
                        </Stack>

                        <Stack direction={isSmallMobile ? "column" : "row"} spacing={isSmallMobile ? 3 : 2}>
                            <TextField
                                label="Breed (Optional)"
                                value={formData.breed}
                                onChange={handleInputChange("breed")}
                                disabled={loading}
                                size="medium"
                                sx={{
                                    flex: 1,
                                    "& .MuiInputBase-root": {
                                        height: isMobile ? 56 : 64,
                                        fontSize: isMobile ? "1rem" : "1.1rem",
                                    },
                                }}
                            />

                            <TextField
                                label="Shed/Location (Optional)"
                                value={formData.shed}
                                onChange={handleInputChange("shed")}
                                disabled={loading}
                                size="medium"
                                sx={{
                                    flex: 1,
                                    "& .MuiInputBase-root": {
                                        height: isMobile ? 56 : 64,
                                        fontSize: isMobile ? "1rem" : "1.1rem",
                                    },
                                }}
                            />
                        </Stack>

                        <Typography variant="h6" sx={{ mt: 2, mb: -1 }}>
                            Purchase Details
                        </Typography>

                        <Stack direction={isSmallMobile ? "column" : "row"} spacing={isSmallMobile ? 3 : 2}>
                            <FormControl
                                fullWidth
                                required
                                size="medium"
                                sx={{
                                    flex: 1,
                                    "& .MuiInputBase-root": {
                                        height: isMobile ? 56 : 64,
                                        fontSize: isMobile ? "1rem" : "1.1rem",
                                    },
                                }}
                            >
                                <InputLabel>Vendor</InputLabel>
                                <Select
                                    value={formData.vendorId}
                                    onChange={(e) => setFormData((prev) => ({ ...prev, vendorId: e.target.value }))}
                                    label="Vendor"
                                    disabled={loading || vendorsLoading}
                                    required
                                >
                                    <MenuItem value="">
                                        <em>Select a vendor</em>
                                    </MenuItem>
                                    {vendors.map((vendor) => (
                                        <MenuItem key={vendor.id} value={vendor.id}>
                                            {vendor.name}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>

                            <TextField
                                label="Initial Cost"
                                type="number"
                                value={formData.initialCost}
                                onChange={handleInputChange("initialCost")}
                                inputProps={{ min: 0.01, step: 0.01 }}
                                disabled={loading}
                                required
                                size="medium"
                                sx={{
                                    flex: 1,
                                    "& .MuiInputBase-root": {
                                        height: isMobile ? 56 : 64,
                                        fontSize: isMobile ? "1rem" : "1.1rem",
                                    },
                                }}
                            />
                        </Stack>

                        <Box
                            sx={{
                                display: "flex",
                                justifyContent: "center",
                                alignItems: "center",
                                p: { xs: 2.5, sm: 3 },
                                bgcolor: "primary.50",
                                borderRadius: 2,
                                border: 1,
                                borderColor: "primary.200",
                            }}
                        >
                            <Typography
                                variant={isSmallMobile ? "h6" : "h5"}
                                sx={{
                                    fontWeight: "bold",
                                    color: "primary.dark",
                                }}
                            >
                                Total Population: {totalPopulation}
                            </Typography>
                        </Box>
                    </Stack>
                </Box>
            </DialogContent>

            <DialogActions
                sx={{
                    px: { xs: 2, sm: 3 },
                    py: { xs: 2, sm: 3 },
                    gap: 2,
                    flexDirection: isSmallMobile ? "column-reverse" : "row",
                }}
            >
                <Button
                    onClick={onClose}
                    disabled={loading}
                    color="inherit"
                    size="large"
                    sx={{
                        minWidth: isSmallMobile ? "100%" : 120,
                        height: { xs: 48, sm: 44 },
                    }}
                >
                    Cancel
                </Button>
                <Button
                    onClick={handleSubmit}
                    variant="contained"
                    disabled={
                        loading ||
                        !formData.name ||
                        !formData.vendorId ||
                        !formData.initialCost ||
                        totalPopulation === 0
                    }
                    startIcon={loading ? <CircularProgress size={20} /> : null}
                    size="large"
                    sx={{
                        minWidth: isSmallMobile ? "100%" : 140,
                        height: { xs: 48, sm: 44 },
                    }}
                >
                    {loading ? "Creating..." : "Create Batch"}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
