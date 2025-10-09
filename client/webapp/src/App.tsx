import { ThemeProvider, createTheme, CssBaseline } from "@mui/material";
import type { Batch } from "./types/batch";
import BatchList from "./components/BatchList";

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
    const handleBatchClick = (batch: Batch) => {
        console.log("Batch selected:", batch);
        // TODO: Navigate to batch details page
        alert(`Opening details for batch: ${batch.name}`);
    };

    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <BatchList onBatchClick={handleBatchClick} />
        </ThemeProvider>
    );
}

export default App;
