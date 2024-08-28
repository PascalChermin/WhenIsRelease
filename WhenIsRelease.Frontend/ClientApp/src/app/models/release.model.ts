import { Region } from './region.model';
import { Platform } from './platform.model';

export interface Release {
  id: number;
  sourceId: number;
  name: string;
  url: string;
  image: string;
  platform: Platform;
  region: Region;
  releaseDate: Date;
  lastUpdate: Date;
}
