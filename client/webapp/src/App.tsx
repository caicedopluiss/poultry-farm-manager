import { createTheme } from "@mui/material";
import { AppProvider } from "@toolpad/core/AppProvider";
import { DashboardLayout } from "@toolpad/core/DashboardLayout";
import type { Navigation } from "@toolpad/core";
import { useNavigate, useLocation, Routes, Route } from "react-router-dom";
import { useMemo } from "react";
import GroupsIcon from "@mui/icons-material/Groups";
import Inventory2Icon from "@mui/icons-material/Inventory2";
import AccountBalanceIcon from "@mui/icons-material/AccountBalance";
import { BatchListPage, BatchDetailPage, NotFoundPage, AssetDetailPage, ProductDetailPage } from "@/pages";
import InventoryPage from "@/pages/InventoryPage";
import PersonListPage from "@/pages/PersonListPage";
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
    {
        segment: "inventory",
        title: "Inventory",
        icon: <Inventory2Icon />,
    },
    {
        segment: "finance",
        title: "Finance",
        icon: <AccountBalanceIcon />,
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
                        <Route path="/inventory" element={<InventoryPage />} />
                        <Route path="/inventory/assets/:id" element={<AssetDetailPage />} />
                        <Route path="/inventory/products/:id" element={<ProductDetailPage />} />
                        <Route path="/finance" element={<PersonListPage />} />
                        <Route path="*" element={<NotFoundPage />} />
                    </Routes>
                </DashboardLayout>
            </BatchesProvider>
        </AppProvider>
    );
}

export default App;
