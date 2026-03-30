import { describe, expect, it, vi } from 'vitest';

vi.mock('./api', () => {
  return {
    default: {
      get: vi.fn(),
    },
  };
});

import api from './api';
import dashboardService from './dashboardService';

describe('dashboardService', () => {
  it('calls /dashboard/summary and returns response data', async () => {
    const payload = {
      totalHumidors: 1,
      totalCigars: 2,
      totalCapacity: 25,
      averageFillPercent: 8.5,
      brandBreakdown: [],
      recentReviews: [],
    };

    vi.mocked(api.get).mockResolvedValueOnce({ data: payload } as any);

    const res = await dashboardService.getDashboardSummary();

    expect(api.get).toHaveBeenCalledWith('/dashboard/summary');
    expect(res).toEqual(payload);
  });
});

