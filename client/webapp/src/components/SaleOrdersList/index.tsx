import { useState } from "react";
import {
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TableSortLabel,
    Paper,
    Chip,
    Typography,
    Box,
    Button,
    IconButton,
    Tooltip,
    Collapse,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
} from "@mui/material";
import {
    Cancel as CancelIcon,
    ExpandMore as ExpandIcon,
    ExpandLess as CollapseIcon,
    Payment as PaymentIcon,
    MoreVert as MoreVertIcon,
} from "@mui/icons-material";
import moment from "moment";
import type { SaleOrder, SaleOrderStatus } from "@/types/saleOrder";

interface SaleOrdersListProps {
    saleOrders: SaleOrder[];
    onAddPayment: (order: SaleOrder) => void;
    onCancel: (order: SaleOrder) => void;
}

function computeStatus(order: SaleOrder): SaleOrderStatus {
    if (order.status === "Cancelled") return "Cancelled";
    if (order.totalPaid >= order.totalAmount) return "Paid";
    if (order.totalPaid > 0) return "PartiallyPaid";
    return "Pending";
}

function StatusChip({ status }: { status: SaleOrderStatus }) {
    const map: Record<SaleOrderStatus, { label: string; color: "warning" | "info" | "success" | "error" }> = {
        Pending: { label: "Pending", color: "warning" },
        PartiallyPaid: { label: "Partially Paid", color: "info" },
        Paid: { label: "Paid", color: "success" },
        Cancelled: { label: "Cancelled", color: "error" },
    };
    const { label, color } = map[status] ?? { label: status, color: "warning" };
    return <Chip label={label} color={color} size="small" />;
}

function OrderActionsDialog({
    order,
    open,
    onClose,
    onAddPayment,
    onCancel,
}: {
    order: SaleOrder;
    open: boolean;
    onClose: () => void;
    onAddPayment: (o: SaleOrder) => void;
    onCancel: (o: SaleOrder) => void;
}) {
    const [confirming, setConfirming] = useState(false);
    const effectiveStatus = computeStatus(order);
    const canPay = effectiveStatus === "Pending" || effectiveStatus === "PartiallyPaid";
    const canCancel = effectiveStatus === "Pending" || effectiveStatus === "PartiallyPaid";

    function handleClose() {
        setConfirming(false);
        onClose();
    }

    function handleAddPayment() {
        handleClose();
        onAddPayment(order);
    }

    function handleConfirmCancel() {
        handleClose();
        onCancel(order);
    }

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
            <DialogTitle>{confirming ? "Confirm Cancellation" : "Order Actions"}</DialogTitle>
            <DialogContent>
                {confirming ? (
                    <Typography sx={{ pt: 1 }}>
                        Are you sure you want to cancel the order for <strong>{order.customerFullName}</strong>? This
                        cannot be undone.
                    </Typography>
                ) : (
                    <Box sx={{ display: "flex", flexDirection: "column", gap: 1.5, pt: 1 }}>
                        <Typography variant="body2" color="text.secondary">
                            {order.customerFullName} — {moment(order.date).format("MMM DD, YYYY")}
                        </Typography>
                        {canPay && (
                            <Button
                                variant="contained"
                                color="success"
                                startIcon={<PaymentIcon />}
                                onClick={handleAddPayment}
                                fullWidth
                            >
                                Add Payment
                            </Button>
                        )}
                        {canCancel && (
                            <Button
                                variant="outlined"
                                color="error"
                                startIcon={<CancelIcon />}
                                onClick={() => setConfirming(true)}
                                fullWidth
                            >
                                Cancel Order
                            </Button>
                        )}
                    </Box>
                )}
            </DialogContent>
            <DialogActions>
                {confirming ? (
                    <>
                        <Button onClick={() => setConfirming(false)}>Back</Button>
                        <Button variant="contained" color="error" onClick={handleConfirmCancel}>
                            Yes, Cancel Order
                        </Button>
                    </>
                ) : (
                    <Button onClick={handleClose}>Close</Button>
                )}
            </DialogActions>
        </Dialog>
    );
}

function SaleOrderRow({
    order,
    onAddPayment,
    onCancel,
}: {
    order: SaleOrder;
    onAddPayment: (o: SaleOrder) => void;
    onCancel: (o: SaleOrder) => void;
}) {
    const [expanded, setExpanded] = useState(false);
    const [actionsOpen, setActionsOpen] = useState(false);
    const effectiveStatus = computeStatus(order);
    const canAct = effectiveStatus === "Pending" || effectiveStatus === "PartiallyPaid";

    return (
        <>
            <TableRow hover>
                <TableCell>
                    <IconButton size="small" onClick={() => setExpanded((v) => !v)}>
                        {expanded ? <CollapseIcon fontSize="small" /> : <ExpandIcon fontSize="small" />}
                    </IconButton>
                </TableCell>
                <TableCell>{moment(order.date).format("MMM DD, YYYY")}</TableCell>
                <TableCell>{order.customerFullName}</TableCell>
                <TableCell>
                    <StatusChip status={effectiveStatus} />
                </TableCell>
                <TableCell align="right">{order.totalWeight.toFixed(3)} kg</TableCell>
                <TableCell align="right">${order.pricePerUnit.toFixed(3)}</TableCell>
                <TableCell align="right">${order.totalAmount.toFixed(3)}</TableCell>
                <TableCell align="right" sx={{ color: "success.main", fontWeight: 500 }}>
                    ${order.totalPaid.toFixed(3)}
                </TableCell>
                <TableCell align="right" sx={{ color: order.pendingAmount > 0 ? "error.main" : "text.secondary" }}>
                    ${order.pendingAmount.toFixed(3)}
                </TableCell>
                <TableCell>
                    {canAct && (
                        <Tooltip title="Manage order">
                            <IconButton size="small" onClick={() => setActionsOpen(true)}>
                                <MoreVertIcon fontSize="small" />
                            </IconButton>
                        </Tooltip>
                    )}
                    <OrderActionsDialog
                        order={order}
                        open={actionsOpen}
                        onClose={() => setActionsOpen(false)}
                        onAddPayment={onAddPayment}
                        onCancel={onCancel}
                    />
                </TableCell>
            </TableRow>

            {/* Expanded details: items + payments */}
            <TableRow>
                <TableCell colSpan={10} sx={{ py: 0 }}>
                    <Collapse in={expanded} unmountOnExit>
                        <Box sx={{ p: 2, display: "flex", gap: 4, flexWrap: "wrap" }}>
                            {/* Items */}
                            <Box sx={{ flex: 1, minWidth: 280 }}>
                                <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600 }}>
                                    Processed Chickens ({order.items.length})
                                </Typography>
                                <Table size="small">
                                    <TableHead>
                                        <TableRow>
                                            <TableCell sx={{ fontWeight: 600 }}>#</TableCell>
                                            <TableCell sx={{ fontWeight: 600 }}>Weight</TableCell>
                                            <TableCell sx={{ fontWeight: 600 }}>UoM</TableCell>
                                            <TableCell sx={{ fontWeight: 600 }}>Processed</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {order.items.map((item, i) => (
                                            <TableRow key={item.id}>
                                                <TableCell>{i + 1}</TableCell>
                                                <TableCell>{item.weight.toFixed(3)}</TableCell>
                                                <TableCell>{item.unitOfMeasure}</TableCell>
                                                <TableCell>
                                                    {moment(item.processedDate).format("MMM DD, YYYY")}
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </Box>

                            {/* Payments */}
                            <Box sx={{ flex: 1, minWidth: 280 }}>
                                <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600 }}>
                                    Payments ({order.payments.length})
                                </Typography>
                                {order.payments.length === 0 ? (
                                    <Typography variant="body2" color="text.secondary">
                                        No payments yet
                                    </Typography>
                                ) : (
                                    <Table size="small">
                                        <TableHead>
                                            <TableRow>
                                                <TableCell sx={{ fontWeight: 600 }}>Date</TableCell>
                                                <TableCell align="right" sx={{ fontWeight: 600 }}>
                                                    Amount
                                                </TableCell>
                                                <TableCell sx={{ fontWeight: 600 }}>Notes</TableCell>
                                            </TableRow>
                                        </TableHead>
                                        <TableBody>
                                            {order.payments.map((p) => (
                                                <TableRow key={p.transactionId}>
                                                    <TableCell>{moment(p.date).format("MMM DD, YYYY")}</TableCell>
                                                    <TableCell align="right">${p.amount.toFixed(3)}</TableCell>
                                                    <TableCell>{p.notes ?? "-"}</TableCell>
                                                </TableRow>
                                            ))}
                                        </TableBody>
                                    </Table>
                                )}
                            </Box>

                            {/* Notes */}
                            {order.notes && (
                                <Box sx={{ width: "100%" }}>
                                    <Typography variant="caption" color="text.secondary">
                                        Notes: {order.notes}
                                    </Typography>
                                </Box>
                            )}
                        </Box>
                    </Collapse>
                </TableCell>
            </TableRow>
        </>
    );
}

type SortKey =
    | "date"
    | "customerFullName"
    | "status"
    | "totalWeight"
    | "pricePerUnit"
    | "totalAmount"
    | "totalPaid"
    | "pendingAmount";
type SortDir = "asc" | "desc";

const STATUS_ORDER: Record<SaleOrderStatus, number> = {
    Pending: 0,
    PartiallyPaid: 1,
    Paid: 2,
    Cancelled: 3,
};

function sortOrders(orders: SaleOrder[], key: SortKey, dir: SortDir): SaleOrder[] {
    return [...orders].sort((a, b) => {
        let cmp = 0;
        switch (key) {
            case "date":
                cmp = a.date.localeCompare(b.date);
                break;
            case "customerFullName":
                cmp = a.customerFullName.localeCompare(b.customerFullName);
                break;
            case "status":
                cmp = STATUS_ORDER[computeStatus(a)] - STATUS_ORDER[computeStatus(b)];
                break;
            case "totalWeight":
                cmp = a.totalWeight - b.totalWeight;
                break;
            case "pricePerUnit":
                cmp = a.pricePerUnit - b.pricePerUnit;
                break;
            case "totalAmount":
                cmp = a.totalAmount - b.totalAmount;
                break;
            case "totalPaid":
                cmp = a.totalPaid - b.totalPaid;
                break;
            case "pendingAmount":
                cmp = a.pendingAmount - b.pendingAmount;
                break;
        }
        return dir === "asc" ? cmp : -cmp;
    });
}

export default function SaleOrdersList({ saleOrders, onAddPayment, onCancel }: SaleOrdersListProps) {
    const [sortKey, setSortKey] = useState<SortKey>("date");
    const [sortDir, setSortDir] = useState<SortDir>("desc");

    function handleSort(key: SortKey) {
        if (sortKey === key) {
            setSortDir((d) => (d === "asc" ? "desc" : "asc"));
        } else {
            setSortKey(key);
            setSortDir("asc");
        }
    }

    if (saleOrders.length === 0) {
        return (
            <Box sx={{ textAlign: "center", py: 4 }}>
                <Typography variant="body1" color="text.secondary">
                    No sale orders yet
                </Typography>
            </Box>
        );
    }

    const sorted = sortOrders(saleOrders, sortKey, sortDir);

    function col(key: SortKey, label: string, align: "left" | "right" = "left") {
        return (
            <TableCell align={align} sx={{ fontWeight: "bold" }} sortDirection={sortKey === key ? sortDir : false}>
                <TableSortLabel
                    active={sortKey === key}
                    direction={sortKey === key ? sortDir : "asc"}
                    onClick={() => handleSort(key)}
                >
                    {label}
                </TableSortLabel>
            </TableCell>
        );
    }

    return (
        <TableContainer component={Paper} variant="outlined">
            <Table>
                <TableHead>
                    <TableRow sx={{ bgcolor: "grey.50" }}>
                        <TableCell width={40} />
                        {col("date", "Date")}
                        {col("customerFullName", "Customer")}
                        {col("status", "Status")}
                        {col("totalWeight", "Total Weight", "right")}
                        {col("pricePerUnit", "Price/Kg", "right")}
                        {col("totalAmount", "Total Value", "right")}
                        {col("totalPaid", "Collected", "right")}
                        {col("pendingAmount", "Pending", "right")}
                        <TableCell sx={{ fontWeight: "bold" }}>Actions</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {sorted.map((order) => (
                        <SaleOrderRow key={order.id} order={order} onAddPayment={onAddPayment} onCancel={onCancel} />
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    );
}
