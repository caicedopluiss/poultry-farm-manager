import { useState, useEffect } from "react";
import {
    Box,
    Typography,
    Button,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    CircularProgress,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    Alert,
    MenuItem,
    Select,
    FormControl,
    InputLabel,
    IconButton,
    Popover,
} from "@mui/material";
import { Add as AddIcon, Info as InfoIcon } from "@mui/icons-material";
import VendorModal from "@/components/VendorModal";
import { getVendors, createVendor, updateVendor } from "@/api/v1/vendors";
import { getPersons } from "@/api/v1/persons";
import type { Vendor, NewVendor, UpdateVendor } from "@/types/vendor";
import type { Person } from "@/types/person";

export default function VendorListPage() {
    const [vendors, setVendors] = useState<Vendor[]>([]);
    const [persons, setPersons] = useState<Person[]>([]);
    const [loading, setLoading] = useState(false);
    const [personsLoading, setPersonsLoading] = useState(false);
    const [selectedVendor, setSelectedVendor] = useState<Vendor | null>(null);
    const [vendorModalOpen, setVendorModalOpen] = useState(false);
    const [createDialogOpen, setCreateDialogOpen] = useState(false);
    const [createLoading, setCreateLoading] = useState(false);
    const [createError, setCreateError] = useState<string | null>(null);
    const [contactInfoAnchor, setContactInfoAnchor] = useState<HTMLElement | null>(null);
    const [selectedContactPerson, setSelectedContactPerson] = useState<Person | null>(null);

    // Form fields for creating a new vendor
    const [newName, setNewName] = useState("");
    const [newLocation, setNewLocation] = useState("");
    const [newContactPersonId, setNewContactPersonId] = useState("");

    useEffect(() => {
        loadVendors();
        loadPersons();
    }, []);

    const loadVendors = async () => {
        setLoading(true);
        try {
            const response = await getVendors();
            setVendors(response.vendors);
        } catch (error) {
            console.error("Failed to load vendors:", error);
        } finally {
            setLoading(false);
        }
    };

    const loadPersons = async () => {
        setPersonsLoading(true);
        try {
            const response = await getPersons();
            setPersons(response.persons);
        } catch (error) {
            console.error("Failed to load persons:", error);
        } finally {
            setPersonsLoading(false);
        }
    };

    const handleVendorClick = (vendor: Vendor) => {
        setSelectedVendor(vendor);
        setVendorModalOpen(true);
    };

    const handleVendorModalClose = () => {
        setVendorModalOpen(false);
        setSelectedVendor(null);
    };

    const handleUpdateVendor = async (id: string, data: UpdateVendor) => {
        await updateVendor(id, data);
        await loadVendors();
    };

    const handleCreateDialogOpen = () => {
        setNewName("");
        setNewLocation("");
        setNewContactPersonId("");
        setCreateError(null);
        setCreateDialogOpen(true);
    };

    const handleCreateDialogClose = () => {
        setCreateDialogOpen(false);
        setCreateError(null);
    };

    const handleCreateVendor = async () => {
        if (!newName.trim()) {
            setCreateError("Vendor name is required");
            return;
        }

        if (!newContactPersonId) {
            setCreateError("Contact person is required");
            return;
        }

        setCreateLoading(true);
        setCreateError(null);

        try {
            const newVendorData: NewVendor = {
                name: newName.trim(),
                location: newLocation.trim() || undefined,
                contactPersonId: newContactPersonId,
            };

            await createVendor(newVendorData);
            await loadVendors();
            handleCreateDialogClose();
        } catch (error) {
            console.error("Failed to create vendor:", error);
            setCreateError("Failed to create vendor. Please try again.");
        } finally {
            setCreateLoading(false);
        }
    };

    const getContactPersonName = (vendor: Vendor) => {
        if (vendor.contactPerson) {
            return `${vendor.contactPerson.firstName} ${vendor.contactPerson.lastName}`;
        }
        return "-";
    };

    const handleContactInfoClick = (event: React.MouseEvent<HTMLElement>, person: Person) => {
        event.stopPropagation();
        setSelectedContactPerson(person);
        setContactInfoAnchor(event.currentTarget);
    };

    const handleContactInfoClose = () => {
        setContactInfoAnchor(null);
        setSelectedContactPerson(null);
    };

    const contactInfoOpen = Boolean(contactInfoAnchor);

    return (
        <>
            <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
                <Typography variant="h4" component="h1">
                    Vendors
                </Typography>
                <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreateDialogOpen}>
                    Add Vendor
                </Button>
            </Box>

            {loading ? (
                <Box sx={{ display: "flex", justifyContent: "center", mt: 4 }}>
                    <CircularProgress />
                </Box>
            ) : (
                <TableContainer component={Paper}>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell>Name</TableCell>
                                <TableCell>Location</TableCell>
                                <TableCell>Contact Person</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {vendors.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={3} align="center">
                                        No vendors found
                                    </TableCell>
                                </TableRow>
                            ) : (
                                vendors.map((vendor) => (
                                    <TableRow
                                        key={vendor.id}
                                        hover
                                        sx={{ cursor: "pointer" }}
                                        onClick={() => handleVendorClick(vendor)}
                                    >
                                        <TableCell>{vendor.name}</TableCell>
                                        <TableCell>{vendor.location || "-"}</TableCell>
                                        <TableCell>
                                            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                                {getContactPersonName(vendor)}
                                                {vendor.contactPerson && (
                                                    <IconButton
                                                        size="small"
                                                        onClick={(e) =>
                                                            handleContactInfoClick(e, vendor.contactPerson!)
                                                        }
                                                        sx={{ ml: 0.5 }}
                                                    >
                                                        <InfoIcon fontSize="small" />
                                                    </IconButton>
                                                )}
                                            </Box>
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}

            {/* Contact Person Info Popover */}
            <Popover
                open={contactInfoOpen}
                anchorEl={contactInfoAnchor}
                onClose={handleContactInfoClose}
                anchorOrigin={{
                    vertical: "bottom",
                    horizontal: "right",
                }}
                transformOrigin={{
                    vertical: "top",
                    horizontal: "right",
                }}
            >
                <Box sx={{ p: 2, minWidth: 250 }}>
                    {selectedContactPerson && (
                        <>
                            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                                Contact Details
                            </Typography>
                            <Typography variant="body2" sx={{ mt: 1 }}>
                                <strong>Name:</strong> {selectedContactPerson.firstName}{" "}
                                {selectedContactPerson.lastName}
                            </Typography>
                            {selectedContactPerson.email && (
                                <Typography variant="body2" sx={{ mt: 0.5 }}>
                                    <strong>Email:</strong> {selectedContactPerson.email}
                                </Typography>
                            )}
                            {selectedContactPerson.phoneNumber && (
                                <Typography variant="body2" sx={{ mt: 0.5 }}>
                                    <strong>Phone:</strong> {selectedContactPerson.phoneNumber}
                                </Typography>
                            )}
                            {selectedContactPerson.location && (
                                <Typography variant="body2" sx={{ mt: 0.5 }}>
                                    <strong>Location:</strong> {selectedContactPerson.location}
                                </Typography>
                            )}
                        </>
                    )}
                </Box>
            </Popover>

            {/* Vendor Detail/Edit Modal */}
            {selectedVendor && (
                <VendorModal
                    open={vendorModalOpen}
                    onClose={handleVendorModalClose}
                    vendor={selectedVendor}
                    persons={persons}
                    onUpdate={handleUpdateVendor}
                />
            )}

            {/* Create Vendor Dialog */}
            <Dialog open={createDialogOpen} onClose={handleCreateDialogClose} maxWidth="sm" fullWidth>
                <DialogTitle>Add New Vendor</DialogTitle>
                <DialogContent>
                    <Box sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
                        {createError && <Alert severity="error">{createError}</Alert>}

                        <TextField
                            label="Vendor Name"
                            value={newName}
                            onChange={(e) => setNewName(e.target.value)}
                            fullWidth
                            required
                        />

                        <TextField
                            label="Location"
                            value={newLocation}
                            onChange={(e) => setNewLocation(e.target.value)}
                            fullWidth
                        />

                        <FormControl fullWidth required>
                            <InputLabel>Contact Person</InputLabel>
                            <Select
                                value={newContactPersonId}
                                onChange={(e) => setNewContactPersonId(e.target.value)}
                                label="Contact Person"
                                disabled={personsLoading}
                            >
                                {persons.map((person) => (
                                    <MenuItem key={person.id} value={person.id}>
                                        {person.firstName} {person.lastName}
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCreateDialogClose} disabled={createLoading}>
                        Cancel
                    </Button>
                    <Button onClick={handleCreateVendor} variant="contained" disabled={createLoading}>
                        {createLoading ? <CircularProgress size={24} /> : "Create"}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
}
