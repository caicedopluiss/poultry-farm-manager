import { ThemeProvider, createTheme, CssBaseline } from "@mui/material";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { BatchListPage, BatchDetailPage, NotFoundPage } from "./pages";
import { BatchesProvider } from "./contexts/batches";

// Create a basic MUI theme
const theme = createTheme({
    palette: {
        mode: "light",
        primary: {
            main: "#1976d2",
        },
        secondary: {
            main: "#dc004e",
        },
    },
});

function App() {
    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <BatchesProvider>
                <Router>
                    <Routes>
                        <Route path="/" element={<BatchListPage />} />
                        <Route path="/batches" element={<BatchListPage />} />
                        <Route path="/batches/:id" element={<BatchDetailPage />} />
                        <Route path="*" element={<NotFoundPage />} />
                    </Routes>
                </Router>
            </BatchesProvider>
        </ThemeProvider>
    );
}

export default App;
