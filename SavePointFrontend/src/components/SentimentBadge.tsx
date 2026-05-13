import React, { useEffect, useState } from 'react';
import { ReviewSentimentDto, SentimentLabel } from '../interfaces/types';
import { reviewService } from '../services/reviewService';

interface Props {
  reviewId: string;
}

const labelStyle: Record<SentimentLabel, string> = {
  POSITIVE: 'badge-success',
  NEGATIVE: 'badge-error',
  NEUTRAL:  'badge-ghost',
  MIXED:    'badge-warning',
};

const labelEmoji: Record<SentimentLabel, string> = {
  POSITIVE: '😊',
  NEGATIVE: '😡',
  NEUTRAL:  '😐',
  MIXED:    '🤔',
};

const SentimentBadge: React.FC<Props> = ({ reviewId }) => {
  const [data, setData] = useState<ReviewSentimentDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    let attempts = 0;
    let timer: number | undefined;

    const poll = async () => {
      try {
        const result = await reviewService.getReviewSentiment(reviewId);
        if (cancelled) return;

        if (result) {
          setData(result);
          setLoading(false);
          return;
        }

        // Lambda hasn't processed yet — retry a few times with a short backoff.
        attempts += 1;
        if (attempts < 6) {
          timer = window.setTimeout(poll, 2000 + attempts * 1000);
        } else {
          setLoading(false);
        }
      } catch {
        if (!cancelled) setLoading(false);
      }
    };

    poll();

    return () => {
      cancelled = true;
      if (timer) clearTimeout(timer);
    };
  }, [reviewId]);

  if (loading) {
    return <span className="badge badge-ghost badge-sm">analyzing…</span>;
  }
  if (!data) {
    return null;
  }

  const dominantPct = Math.round(
    (data.sentiment === 'POSITIVE' ? data.positive
      : data.sentiment === 'NEGATIVE' ? data.negative
      : data.sentiment === 'NEUTRAL'  ? data.neutral
      : data.mixed) * 100
  );

  return (
    <span
      className={`badge badge-sm gap-1 ${labelStyle[data.sentiment]}`}
      title={`Sentiment: ${data.sentiment} (${dominantPct}%) — analysed by AWS Comprehend`}
    >
      {labelEmoji[data.sentiment]} {data.sentiment.toLowerCase()} {dominantPct}%
    </span>
  );
};

export default SentimentBadge;
