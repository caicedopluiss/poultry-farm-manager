import { createTheme } from "@mui/material";
import { AppProvider } from "@toolpad/core/AppProvider";
import { DashboardLayout } from "@toolpad/core/DashboardLayout";
import type { Navigation } from "@toolpad/core";
import { useNavigate, useLocation, Routes, Route } from "react-router-dom";
import { useMemo } from "react";
import GroupsIcon from "@mui/icons-material/Groups";
import { BatchListPage, BatchDetailPage, NotFoundPage } from "@/pages";
import { BatchesProvider } from "@/contexts/batches";

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

// Define navigation structure for Toolpad
const NAVIGATION: Navigation = [
    {
        segment: "batches",
        title: "Batches",
        icon: <GroupsIcon />,
    },
];

function App() {
    const navigate = useNavigate();
    const location = useLocation();

    // Create router for Toolpad
    const router = useMemo(() => {
        return {
            pathname: location.pathname,
            searchParams: new URLSearchParams(location.search),
            navigate: (path: string | URL) => navigate(String(path)),
        };
    }, [location, navigate]);

    return (
        <AppProvider
            theme={theme}
            navigation={NAVIGATION}
            router={router}
            branding={{
                title: "Poultry Farm Manager",
            }}
        >
            <BatchesProvider>
                <DashboardLayout>
                    <Routes>
                        <Route path="/" element={<BatchListPage />} />
                        <Route path="/batches" element={<BatchListPage />} />
                        <Route path="/batches/:id" element={<BatchDetailPage />} />
                        <Route path="*" element={<NotFoundPage />} />
                    </Routes>
                </DashboardLayout>
            </BatchesProvider>
        </AppProvider>
    );
}

export default App;
