import { useState } from "react";
import { Box, Container, Tabs, Tab } from "@mui/material";
import { People as PeopleIcon, Business as BusinessIcon } from "@mui/icons-material";
import PersonListPage from "./PersonListPage";
import VendorListPage from "./VendorListPage";

export default function FinancePage() {
    const [currentTab, setCurrentTab] = useState(0);

    const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
        setCurrentTab(newValue);
    };

    return (
        <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
            <Box sx={{ borderBottom: 1, borderColor: "divider", mb: 3 }}>
                <Tabs value={currentTab} onChange={handleTabChange}>
                    <Tab icon={<PeopleIcon />} label="Persons" iconPosition="start" />
                    <Tab icon={<BusinessIcon />} label="Vendors" iconPosition="start" />
                </Tabs>
            </Box>

            <Box sx={{ mt: 3 }}>
                {currentTab === 0 && <PersonListPage />}
                {currentTab === 1 && <VendorListPage />}
            </Box>
        </Container>
    );
}
