import { useState } from 'react';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { useAuditLogs } from '@/services/chikexams.hooks';
import { Search, ChevronLeft, ChevronRight } from 'lucide-react';
import { CircularProgress } from '@mui/material';

const PAGE_SIZE = 20;

export const AuditLogs = ({
  searchAuditLogs = ioc((keys) => keys.searchAuditLogs) || chikexamsService.searchAuditLogs,
}: {
  searchAuditLogs?: typeof chikexamsService.searchAuditLogs;
}) => {
  const [search, setSearch] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [page, setPage] = useState(1);

  const { data: logs, isLoading } = useAuditLogs({
    params: {
      Page: page,
      PageSize: PAGE_SIZE,
      'DateRange.From': fromDate || undefined,
      'DateRange.To': toDate || undefined,
    },
    searchAuditLogs,
  });

  const filteredLogs = (logs ?? []).filter((log) => {
    if (!search) return true;
    const q = search.toLowerCase();
    return (
      (log.user?.username?.toLowerCase().includes(q) ?? false) ||
      (log.service?.toLowerCase().includes(q) ?? false)
    );
  });

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold" style={{ color: '#211A1E' }}>
          Audit Logs
        </h1>
      </div>

      <div className="flex items-center gap-3 mb-4">
        <div
          className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-white"
          style={{ border: '1px solid #E5E7EB' }}
        >
          <Search size={16} style={{ color: '#6B7280' }} />
          <input
            type="text"
            placeholder="Search by user or service..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="text-sm outline-none bg-transparent"
            style={{ color: '#211A1E', width: 220 }}
          />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm" style={{ color: '#6B7280' }}>From</label>
          <input
            type="date"
            value={fromDate}
            onChange={(e) => { setFromDate(e.target.value); setPage(1); }}
            className="text-sm px-2 py-1.5 rounded-lg bg-white"
            style={{ border: '1px solid #E5E7EB', color: '#211A1E' }}
          />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm" style={{ color: '#6B7280' }}>To</label>
          <input
            type="date"
            value={toDate}
            onChange={(e) => { setToDate(e.target.value); setPage(1); }}
            className="text-sm px-2 py-1.5 rounded-lg bg-white"
            style={{ border: '1px solid #E5E7EB', color: '#211A1E' }}
          />
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm" style={{ border: '1px solid #E5E7EB' }}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr style={{ borderBottom: '1px solid #E5E7EB' }}>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>User</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Service</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Entity ID</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase" style={{ color: '#6B7280' }}>Date</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={4} className="px-4 py-8 text-center">
                    <CircularProgress size={24} />
                  </td>
                </tr>
              ) : filteredLogs.length === 0 ? (
                <tr>
                  <td colSpan={4} className="px-4 py-8 text-center text-sm" style={{ color: '#6B7280' }}>
                    No audit logs found
                  </td>
                </tr>
              ) : (
                filteredLogs.map((log) => (
                  <tr
                    key={log.id}
                    style={{ borderBottom: '1px solid #F3F4F6' }}
                    className="hover:bg-slate-50 transition-colors"
                  >
                    <td className="px-4 py-3 text-sm font-medium" style={{ color: '#211A1E' }}>
                      {log.user?.username ?? `User ${log.userId}`}
                    </td>
                    <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                      {log.service ?? '—'}
                    </td>
                    <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                      {log.entityId}
                    </td>
                    <td className="px-4 py-3 text-sm" style={{ color: '#6B7280' }}>
                      {new Date(log.createdAt).toLocaleDateString()}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="flex items-center justify-between px-4 py-3" style={{ borderTop: '1px solid #E5E7EB' }}>
          <span className="text-sm" style={{ color: '#6B7280' }}>
            Page {page}
          </span>
          <div className="flex items-center gap-2">
            <button
              className="p-1 rounded hover:bg-slate-100 disabled:opacity-40"
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
            >
              <ChevronLeft size={16} />
            </button>
            <button
              className="p-1 rounded hover:bg-slate-100 disabled:opacity-40"
              onClick={() => setPage((p) => p + 1)}
              disabled={(logs?.length ?? 0) < PAGE_SIZE}
            >
              <ChevronRight size={16} />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
