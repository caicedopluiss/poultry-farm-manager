import { useNavigate } from "react-router-dom";
import type { Batch } from "../types/batch";
import BatchList from "../components/BatchList";

export default function BatchListPage() {
    const navigate = useNavigate();

    const handleBatchClick = (batch: Batch) => {
        navigate(`/batches/${batch.id}`);
    };

    return <BatchList onBatchClick={handleBatchClick} />;
}
