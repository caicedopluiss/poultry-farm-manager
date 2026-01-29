import {
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    Chip,
    Typography,
    Box,
} from "@mui/material";
import { TrendingUp as IncomeIcon, TrendingDown as ExpenseIcon } from "@mui/icons-material";
import moment from "moment";
import type { Transaction } from "@/types/transaction";

interface TransactionsTableProps {
    transactions: Transaction[];
}

export default function TransactionsTable({ transactions }: TransactionsTableProps) {
    if (transactions.length === 0) {
        return (
            <Box sx={{ textAlign: "center", py: 4 }}>
                <Typography variant="body1" color="text.secondary">
                    No transactions found
                </Typography>
            </Box>
        );
    }

    return (
        <TableContainer component={Paper} variant="outlined">
            <Table>
                <TableHead>
                    <TableRow sx={{ bgcolor: "grey.50" }}>
                        <TableCell sx={{ fontWeight: "bold" }}>Date</TableCell>
                        <TableCell sx={{ fontWeight: "bold" }}>Title</TableCell>
                        <TableCell sx={{ fontWeight: "bold" }}>Type</TableCell>
                        <TableCell align="right" sx={{ fontWeight: "bold" }}>
                            Unit Price
                        </TableCell>
                        <TableCell align="right" sx={{ fontWeight: "bold" }}>
                            Quantity
                        </TableCell>
                        <TableCell align="right" sx={{ fontWeight: "bold" }}>
                            Amount
                        </TableCell>
                        <TableCell sx={{ fontWeight: "bold" }}>Details</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {transactions.map((transaction) => (
                        <TableRow key={transaction.id} hover>
                            <TableCell>{moment(transaction.date).format("MMM DD, YYYY")}</TableCell>
                            <TableCell>
                                <Typography variant="body2" sx={{ fontWeight: 500 }}>
                                    {transaction.title}
                                </Typography>
                                {transaction.notes && (
                                    <Typography variant="caption" color="text.secondary">
                                        {transaction.notes}
                                    </Typography>
                                )}
                            </TableCell>
                            <TableCell>
                                {transaction.type === "Income" ? (
                                    <Chip icon={<IncomeIcon />} label="Income" color="success" size="small" />
                                ) : (
                                    <Chip icon={<ExpenseIcon />} label="Expense" color="error" size="small" />
                                )}
                            </TableCell>
                            <TableCell align="right">${transaction.unitPrice.toFixed(2)}</TableCell>
                            <TableCell align="right">{transaction.quantity || "-"}</TableCell>
                            <TableCell align="right">
                                <Typography variant="body2" sx={{ fontWeight: "bold" }}>
                                    ${transaction.transactionAmount.toFixed(2)}
                                </Typography>
                            </TableCell>
                            <TableCell>
                                <Box sx={{ display: "flex", flexDirection: "column", gap: 0.5 }}>
                                    {transaction.vendorName && (
                                        <Typography variant="caption" color="text.secondary">
                                            Vendor: {transaction.vendorName}
                                        </Typography>
                                    )}
                                    {transaction.customerName && (
                                        <Typography variant="caption" color="text.secondary">
                                            Customer: {transaction.customerName}
                                        </Typography>
                                    )}
                                    {transaction.productVariantName && (
                                        <Typography variant="caption" color="text.secondary">
                                            Product: {transaction.productVariantName}
                                        </Typography>
                                    )}
                                </Box>
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    );
}
