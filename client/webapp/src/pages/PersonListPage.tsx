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
} from "@mui/material";
import { Add as AddIcon } from "@mui/icons-material";
import PersonModal from "@/components/PersonModal";
import { getPersons, createPerson, updatePerson } from "@/api/v1/persons";
import type { Person, NewPerson, UpdatePerson } from "@/types/person";

export default function PersonListPage() {
    const [persons, setPersons] = useState<Person[]>([]);
    const [loading, setLoading] = useState(false);
    const [selectedPerson, setSelectedPerson] = useState<Person | null>(null);
    const [personModalOpen, setPersonModalOpen] = useState(false);
    const [createDialogOpen, setCreateDialogOpen] = useState(false);
    const [createLoading, setCreateLoading] = useState(false);
    const [createError, setCreateError] = useState<string | null>(null);

    // Form fields for creating a new person
    const [newFirstName, setNewFirstName] = useState("");
    const [newLastName, setNewLastName] = useState("");
    const [newEmail, setNewEmail] = useState("");
    const [newPhoneNumber, setNewPhoneNumber] = useState("");
    const [newLocation, setNewLocation] = useState("");

    useEffect(() => {
        loadPersons();
    }, []);

    const loadPersons = async () => {
        setLoading(true);
        try {
            const response = await getPersons();
            setPersons(response.persons);
        } catch (error) {
            console.error("Failed to load persons:", error);
        } finally {
            setLoading(false);
        }
    };

    const handlePersonClick = (person: Person) => {
        setSelectedPerson(person);
        setPersonModalOpen(true);
    };

    const handlePersonModalClose = () => {
        setPersonModalOpen(false);
        setSelectedPerson(null);
    };

    const handleUpdatePerson = async (id: string, data: UpdatePerson) => {
        await updatePerson(id, data);
        await loadPersons();
    };

    const handleCreateDialogOpen = () => {
        setNewFirstName("");
        setNewLastName("");
        setNewEmail("");
        setNewPhoneNumber("");
        setNewLocation("");
        setCreateError(null);
        setCreateDialogOpen(true);
    };

    const handleCreateDialogClose = () => {
        setCreateDialogOpen(false);
        setCreateError(null);
    };

    const handleCreatePerson = async () => {
        if (!newFirstName.trim() || !newLastName.trim()) {
            setCreateError("First name and last name are required");
            return;
        }

        setCreateLoading(true);
        setCreateError(null);

        try {
            const newPersonData: NewPerson = {
                firstName: newFirstName.trim(),
                lastName: newLastName.trim(),
                email: newEmail.trim() || undefined,
                phoneNumber: newPhoneNumber.trim() || undefined,
                location: newLocation.trim() || undefined,
            };

            await createPerson(newPersonData);
            await loadPersons();
            handleCreateDialogClose();
        } catch (error) {
            console.error("Failed to create person:", error);
            setCreateError("Failed to create person. Please try again.");
        } finally {
            setCreateLoading(false);
        }
    };

    return (
        <>
            <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
                <Typography variant="h4" component="h1">
                    Persons
                </Typography>
                <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreateDialogOpen}>
                    Add Person
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
                                <TableCell>First Name</TableCell>
                                <TableCell>Last Name</TableCell>
                                <TableCell>Email</TableCell>
                                <TableCell>Phone Number</TableCell>
                                <TableCell>Location</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {persons.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} align="center">
                                        No persons found
                                    </TableCell>
                                </TableRow>
                            ) : (
                                persons.map((person) => (
                                    <TableRow
                                        key={person.id}
                                        hover
                                        sx={{ cursor: "pointer" }}
                                        onClick={() => handlePersonClick(person)}
                                    >
                                        <TableCell>{person.firstName}</TableCell>
                                        <TableCell>{person.lastName}</TableCell>
                                        <TableCell>{person.email || "-"}</TableCell>
                                        <TableCell>{person.phoneNumber || "-"}</TableCell>
                                        <TableCell>{person.location || "-"}</TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}

            {/* Person Detail/Edit Modal */}
            {selectedPerson && (
                <PersonModal
                    open={personModalOpen}
                    onClose={handlePersonModalClose}
                    person={selectedPerson}
                    onUpdate={handleUpdatePerson}
                />
            )}

            {/* Create Person Dialog */}
            <Dialog open={createDialogOpen} onClose={handleCreateDialogClose} maxWidth="sm" fullWidth>
                <DialogTitle>Add New Person</DialogTitle>
                <DialogContent>
                    <Box sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
                        {createError && <Alert severity="error">{createError}</Alert>}

                        <TextField
                            label="First Name"
                            value={newFirstName}
                            onChange={(e) => setNewFirstName(e.target.value)}
                            fullWidth
                            required
                        />

                        <TextField
                            label="Last Name"
                            value={newLastName}
                            onChange={(e) => setNewLastName(e.target.value)}
                            fullWidth
                            required
                        />

                        <TextField
                            label="Email"
                            value={newEmail}
                            onChange={(e) => setNewEmail(e.target.value)}
                            fullWidth
                        />

                        <TextField
                            label="Phone Number"
                            value={newPhoneNumber}
                            onChange={(e) => setNewPhoneNumber(e.target.value)}
                            fullWidth
                        />

                        <TextField
                            label="Location"
                            value={newLocation}
                            onChange={(e) => setNewLocation(e.target.value)}
                            fullWidth
                        />
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCreateDialogClose} disabled={createLoading}>
                        Cancel
                    </Button>
                    <Button onClick={handleCreatePerson} variant="contained" disabled={createLoading}>
                        {createLoading ? <CircularProgress size={24} /> : "Create"}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
}
