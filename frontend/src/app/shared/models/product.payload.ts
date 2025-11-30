import { Product } from './Product';

export interface ProductPayload {
    products: Product[];
    count: number;
}
