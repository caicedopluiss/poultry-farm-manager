import { useNavigate } from "react-router-dom";
import { Container, Typography, Button, Box } from "@mui/material";
import { Home as HomeIcon } from "@mui/icons-material";

export default function NotFoundPage() {
    const navigate = useNavigate();

    return (
        <Container maxWidth="lg" sx={{ py: 8, textAlign: "center" }}>
            <Typography variant="h3" gutterBottom color="primary">
                404 - Page Not Found
            </Typography>
            <Typography variant="h6" color="text.secondary" gutterBottom>
                The page you're looking for doesn't exist.
            </Typography>
            <Box sx={{ mt: 4 }}>
                <Button variant="contained" size="large" startIcon={<HomeIcon />} onClick={() => navigate("/")}>
                    Go to Batches
                </Button>
            </Box>
        </Container>
    );
}
